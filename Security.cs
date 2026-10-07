using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace inventory_management_system_with_POS
{
    // ==========================================================
    // PASSWORD HASHER
    //
    // One-way hashing for account passwords (PBKDF2-HMAC-SHA256).
    // A hash can NOT be turned back into the password - you can
    // only check whether a typed password produces the same hash.
    //
    // Stored format (fits in VARCHAR(255)):
    //   PBKDF2-SHA256$<iterations>$<salt base64>$<hash base64>
    //
    // Every password gets its own random salt, so two users with
    // the same password end up with different hashes. The
    // iteration count is stored inside the hash, so you can raise
    // CurrentIterations later and old hashes still verify (and are
    // upgraded on the next successful login).
    // ==========================================================
    public static class PasswordHasher
    {
        private const string Scheme = "PBKDF2-SHA256";
        private const int SaltSize = 16;   // bytes
        private const int HashSize = 32;   // bytes
        private const int MaxAcceptedIterations = 5000000;

        public const int CurrentIterations = 600000;
        public const int MinimumLength = 8;

        private static string _dummyHash;

        /// <summary>Hashes a password with a fresh random salt.</summary>
        public static string Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] salt = new byte[SaltSize];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, CurrentIterations, HashSize);

            return Scheme + "$" +
                   CurrentIterations + "$" +
                   Convert.ToBase64String(salt) + "$" +
                   Convert.ToBase64String(hash);
        }

        /// <summary>True when the stored value is in the hashed format above.</summary>
        public static bool IsHashed(string stored)
        {
            return stored != null &&
                   stored.StartsWith(Scheme + "$", StringComparison.Ordinal);
        }

        /// <summary>Checks a typed password against a stored hash.</summary>
        public static bool Verify(string password, string stored)
        {
            if (password == null || !IsHashed(stored))
            {
                return false;
            }

            string[] parts = stored.Split('$');

            if (parts.Length != 4)
            {
                return false;
            }

            int iterations;

            if (!int.TryParse(parts[1], out iterations) ||
                iterations < 1 ||
                iterations > MaxAcceptedIterations)
            {
                return false;
            }

            byte[] salt;
            byte[] expected;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length == 0 || expected.Length == 0)
            {
                return false;
            }

            byte[] actual = Derive(password, salt, iterations, expected.Length);

            return FixedTimeEquals(actual, expected);
        }

        /// <summary>
        /// For accounts created BEFORE hashing was added (password stored as
        /// plain text). Used only so old accounts can log in once and be
        /// upgraded to a hash automatically.
        /// </summary>
        public static bool VerifyLegacyPlainText(string password, string stored)
        {
            if (password == null || stored == null || IsHashed(stored))
            {
                return false;
            }

            return FixedTimeEquals(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(stored));
        }

        /// <summary>True when the stored value should be re-hashed (plain text or weaker settings).</summary>
        public static bool NeedsRehash(string stored)
        {
            if (!IsHashed(stored))
            {
                return true;
            }

            string[] parts = stored.Split('$');
            int iterations;

            return parts.Length != 4 ||
                   !int.TryParse(parts[1], out iterations) ||
                   iterations < CurrentIterations;
        }

        /// <summary>
        /// Burns the same time as a real check. Call it when the username does
        /// not exist so response time does not reveal which usernames are valid.
        /// </summary>
        public static void DummyVerify(string password)
        {
            if (_dummyHash == null)
            {
                _dummyHash = Hash("dummy-password-for-timing");
            }

            Verify(password ?? string.Empty, _dummyHash);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations, int length)
        {
            using (Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(
                password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return kdf.GetBytes(length);
            }
        }

        // Compares without stopping at the first different byte,
        // so timing does not leak how much of a value matched.
        internal static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;

            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }

    // ==========================================================
    // DATA ENCRYPTOR
    //
    // Two-way encryption (AES-256-CBC + HMAC-SHA256) for values
    // you need to read back later, such as the database password
    // in app.config. Unlike a hash, this CAN be decrypted.
    //
    // The secret key is created on first use, protected with
    // Windows DPAPI (tied to the current Windows user) and stored in
    //   %LocalAppData%\InventoryPOS\master.key
    // so no key is ever written into the source code.
    //
    // Output looks like:  enc:v1:<base64>
    // The HMAC makes any tampering with the text detectable.
    //
    // NOTE: because the key is tied to the Windows user + machine,
    // a value encrypted here can only be decrypted by the same
    // Windows user on the same PC.
    // ==========================================================
    public static class DataEncryptor
    {
        public const string Prefix = "enc:v1:";

        private const int KeySize = 32;   // bytes (AES-256)
        private const int IvSize = 16;    // bytes
        private const int TagSize = 32;   // bytes (HMAC-SHA256)

        private static readonly byte[] KeyEntropy =
            Encoding.UTF8.GetBytes("InventoryPOS.MasterKey.v1");

        private static readonly object KeyLock = new object();
        private static byte[] _encKey;
        private static byte[] _macKey;

        public static bool IsEncrypted(string value)
        {
            return value != null &&
                   value.StartsWith(Prefix, StringComparison.Ordinal);
        }

        public static string Encrypt(string plainText)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("plainText");
            }

            EnsureKeys();

            byte[] iv;
            byte[] cipher;

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize * 8;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = _encKey;
                aes.GenerateIV();
                iv = aes.IV;

                byte[] data = Encoding.UTF8.GetBytes(plainText);

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    cipher = encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }

            byte[] body = new byte[IvSize + cipher.Length];
            Buffer.BlockCopy(iv, 0, body, 0, IvSize);
            Buffer.BlockCopy(cipher, 0, body, IvSize, cipher.Length);

            byte[] tag = ComputeTag(body);

            byte[] result = new byte[body.Length + TagSize];
            Buffer.BlockCopy(body, 0, result, 0, body.Length);
            Buffer.BlockCopy(tag, 0, result, body.Length, TagSize);

            return Prefix + Convert.ToBase64String(result);
        }

        /// <summary>Decrypts text produced by Encrypt. Throws CryptographicException if invalid or tampered.</summary>
        public static string Decrypt(string encryptedText)
        {
            if (!IsEncrypted(encryptedText))
            {
                throw new CryptographicException(
                    "The text is not in the expected encrypted format (it should start with \"" + Prefix + "\").");
            }

            EnsureKeys();

            byte[] all;

            try
            {
                all = Convert.FromBase64String(encryptedText.Substring(Prefix.Length));
            }
            catch (FormatException)
            {
                throw new CryptographicException("The encrypted text is damaged.");
            }

            if (all.Length < IvSize + 16 + TagSize)
            {
                throw new CryptographicException("The encrypted text is too short.");
            }

            int bodyLength = all.Length - TagSize;

            byte[] body = new byte[bodyLength];
            byte[] tag = new byte[TagSize];
            Buffer.BlockCopy(all, 0, body, 0, bodyLength);
            Buffer.BlockCopy(all, bodyLength, tag, 0, TagSize);

            // Check the HMAC BEFORE decrypting.
            if (!PasswordHasher.FixedTimeEquals(ComputeTag(body), tag))
            {
                throw new CryptographicException(
                    "The text was modified, or it was encrypted on a different Windows user or PC.");
            }

            byte[] iv = new byte[IvSize];
            byte[] cipher = new byte[bodyLength - IvSize];
            Buffer.BlockCopy(body, 0, iv, 0, IvSize);
            Buffer.BlockCopy(body, IvSize, cipher, 0, cipher.Length);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize * 8;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = _encKey;
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                    return Encoding.UTF8.GetString(plain);
                }
            }
        }

        private static byte[] ComputeTag(byte[] body)
        {
            using (HMACSHA256 hmac = new HMACSHA256(_macKey))
            {
                return hmac.ComputeHash(body);
            }
        }

        private static void EnsureKeys()
        {
            if (_encKey != null && _macKey != null)
            {
                return;
            }

            lock (KeyLock)
            {
                if (_encKey != null && _macKey != null)
                {
                    return;
                }

                byte[] master = LoadOrCreateMasterKey();

                // Derive two separate keys from the master key
                // (one to encrypt, one to authenticate).
                using (HMACSHA256 kdf = new HMACSHA256(master))
                {
                    _encKey = kdf.ComputeHash(Encoding.UTF8.GetBytes("InventoryPOS.enc"));
                    _macKey = kdf.ComputeHash(Encoding.UTF8.GetBytes("InventoryPOS.mac"));
                }
            }
        }

        private static byte[] LoadOrCreateMasterKey()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "InventoryPOS");

            string file = Path.Combine(folder, "master.key");

            if (File.Exists(file))
            {
                byte[] protectedKey = File.ReadAllBytes(file);

                return ProtectedData.Unprotect(
                    protectedKey, KeyEntropy, DataProtectionScope.CurrentUser);
            }

            Directory.CreateDirectory(folder);

            byte[] key = new byte[KeySize];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            File.WriteAllBytes(
                file,
                ProtectedData.Protect(key, KeyEntropy, DataProtectionScope.CurrentUser));

            return key;
        }
    }
}
