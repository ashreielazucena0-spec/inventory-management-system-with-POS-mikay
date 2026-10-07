using System;

namespace inventory_management_system_with_POS
{
    // Who is logged in. Filled by frmLoginform after a successful login.
    // acc_type values come straight from tbl_account.acc_type:
    //   Admin, Cashier, Inventory Staff, Manager
    public static class Session
    {
        public const string Admin = "Admin";
        public const string Cashier = "Cashier";
        public const string InventoryStaff = "Inventory Staff";
        public const string Manager = "Manager";

        private static int _accountId;
        private static bool _loggedIn;

        public static string AccountName = "";
        public static string AccountType = "";

        public static void Start(int accountId, string accountName, string accountType)
        {
            _accountId = accountId;
            _loggedIn = true;
            AccountName = accountName ?? "";
            AccountType = accountType ?? "";
        }

        public static void End()
        {
            _accountId = 0;
            _loggedIn = false;
            AccountName = "";
            AccountType = "";
        }

        public static bool IsLoggedIn
        {
            get { return _loggedIn; }
        }

        // Value for tbl_sale.cashier_id / tbl_stock_movement.performed_by.
        // null when nobody is logged in (both columns allow NULL).
        public static int? AccountId
        {
            get
            {
                if (_loggedIn)
                {
                    return _accountId;
                }

                return null;
            }
        }

        // Admin may do everything; other roles only what is listed.
        public static bool Can(params string[] allowedRoles)
        {
            if (!_loggedIn)
            {
                return false;
            }

            if (string.Equals(AccountType, Admin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (string role in allowedRoles)
            {
                if (string.Equals(AccountType, role, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
