using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    // One line in the POS cart.
    public class CartLine
    {
        public string ProductId;
        public string ProductName;
        public int Quantity;
        public decimal UnitPrice;
        public decimal LineDiscount;

        public decimal LineTotal
        {
            get { return Quantity * UnitPrice - LineDiscount; }
        }
    }

    // All database access for products, stock, sales and the dashboard.
    // Every method opens its own short-lived connection, so it never
    // collides with the shared connection used by the login/account screens.
    //
    // Tables used (see inventory_system.sql):
    //   tbl_category, tbl_supplier, tbl_product,
    //   tbl_sale, tbl_sale_item, tbl_stock_movement, vw_product_stock
    //
    // Stock is never stored on the product. Current stock = the view
    // vw_product_stock = SUM(IN) - SUM(OUT) of tbl_stock_movement.
    public static class InventoryData
    {
        // Rows with stock at or below this are highlighted in the stock list.
        public const int LowStockLevel = 10;

        // tbl_sale.tax_amount. 0 = no tax. Change if the shop charges tax.
        public const decimal TaxRate = 0m;

        private static bool _defaultsDone;

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------
        private static MySqlConnection OpenConnection()
        {
            MySqlConnection c = new MySqlConnection(MySQL_Dataabase.BuildConnectionString());
            c.Open();
            return c;
        }

        private static MySqlParameter P(string name, object value)
        {
            return new MySqlParameter(name, value == null ? DBNull.Value : value);
        }

        private static object TextOrNull(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return DBNull.Value;
            }

            return s.Trim();
        }

        private static object DateOrNull(DateTime? d)
        {
            if (d.HasValue)
            {
                return d.Value.Date;
            }

            return DBNull.Value;
        }

        private static object DecimalOrNull(decimal? d)
        {
            if (d.HasValue)
            {
                return d.Value;
            }

            return DBNull.Value;
        }

        private static object UserOrNull()
        {
            int? id = Session.AccountId;

            if (id.HasValue)
            {
                return id.Value;
            }

            return DBNull.Value;
        }

        private static DataTable Query(string sql, params MySqlParameter[] ps)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(sql, c))
            {
                foreach (MySqlParameter p in ps)
                {
                    cmd.Parameters.Add(p);
                }

                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        // ------------------------------------------------------------
        // Categories / suppliers
        // ------------------------------------------------------------

        // The schema has no seed data. If tbl_category is empty the
        // categories the old screens used are created once.
        public static void EnsureDefaults()
        {
            if (_defaultsDone)
            {
                return;
            }

            using (MySqlConnection c = OpenConnection())
            {
                long count;

                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM tbl_category", c))
                {
                    count = Convert.ToInt64(cmd.ExecuteScalar());
                }

                if (count == 0)
                {
                    using (MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO tbl_category (category_id, category_name) VALUES " +
                        "('CAT001','Food'),('CAT002','Drinks'),('CAT003','Grocery')," +
                        "('CAT004','Personal Care'),('CAT005','Other')", c))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            _defaultsDone = true;
        }

        public static DataTable GetCategories()
        {
            return Query("SELECT category_id, category_name FROM tbl_category ORDER BY category_name");
        }

        public static DataTable GetSuppliers()
        {
            return Query("SELECT supplier_id, supplier_name FROM tbl_supplier ORDER BY supplier_name");
        }

        // ------------------------------------------------------------
        // Products
        // ------------------------------------------------------------
        private const string ProductSelect = @"
            SELECT p.product_id,
                   p.product_name,
                   c.category_name,
                   p.category_id,
                   p.barcode,
                   p.cost_price,
                   p.selling_price,
                   p.expiry_date,
                   COALESCE(s.current_quantity, 0) AS stock
            FROM tbl_product p
            INNER JOIN tbl_category c ON c.category_id = p.category_id
            LEFT JOIN vw_product_stock s ON s.product_id = p.product_id
            WHERE p.product_status = 'Active' ";

        public static DataTable GetProducts(string search)
        {
            return Query(
                ProductSelect +
                @" AND (p.product_id LIKE @s
                     OR p.product_name LIKE @s
                     OR p.barcode LIKE @s
                     OR c.category_name LIKE @s)
                   ORDER BY p.product_name",
                P("@s", "%" + (search ?? "").Trim() + "%"));
        }

        // Barcode or product id (exact match). Returns null when not found.
        public static DataRow FindProductByCode(string code)
        {
            DataTable dt = Query(
                ProductSelect + " AND (p.barcode = @k OR p.product_id = @k) LIMIT 1",
                P("@k", (code ?? "").Trim()));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // Exact name first, otherwise the first name that contains the text.
        public static DataRow FindProductByName(string name)
        {
            string text = (name ?? "").Trim();

            DataTable dt = Query(
                ProductSelect +
                @" AND p.product_name LIKE @like
                   ORDER BY (p.product_name = @exact) DESC, p.product_name
                   LIMIT 1",
                P("@like", "%" + text + "%"),
                P("@exact", text));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static List<string> GetActiveProductNames()
        {
            List<string> names = new List<string>();

            DataTable dt = Query(
                "SELECT product_name FROM tbl_product WHERE product_status = 'Active' ORDER BY product_name");

            foreach (DataRow row in dt.Rows)
            {
                names.Add(Convert.ToString(row[0]));
            }

            return names;
        }

        // P0001, P0002, ... (product_id is VARCHAR(10)).
        public static string NextProductId()
        {
            DataTable dt = Query(
                @"SELECT COALESCE(MAX(CAST(SUBSTRING(product_id, 2) AS UNSIGNED)), 0)
                  FROM tbl_product
                  WHERE product_id REGEXP '^P[0-9]+$'");

            long last = Convert.ToInt64(dt.Rows[0][0]);

            return "P" + (last + 1).ToString("D4");
        }

        // With expiry: saved on the product AND the opening stock is booked as
        // a batch with that expiry date (what frmExpireditem reads).
        public static void InsertProduct(
            string id, string name, string barcode, string categoryId,
            decimal costPrice, decimal sellingPrice, int openingStock, DateTime? expiry)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlTransaction tx = c.BeginTransaction())
            {
                using (MySqlCommand cmd = new MySqlCommand(
                    @"INSERT INTO tbl_product
                        (product_id, product_name, barcode, category_id,
                         cost_price, selling_price, expiry_date, product_status)
                      VALUES
                        (@id, @name, @barcode, @cat, @cost, @price, @expiry, 'Active')", c, tx))
                {
                    cmd.Parameters.Add(P("@id", id));
                    cmd.Parameters.Add(P("@name", name.Trim()));
                    cmd.Parameters.Add(P("@barcode", TextOrNull(barcode)));
                    cmd.Parameters.Add(P("@cat", categoryId));
                    cmd.Parameters.Add(P("@cost", costPrice));
                    cmd.Parameters.Add(P("@price", sellingPrice));
                    cmd.Parameters.Add(P("@expiry", DateOrNull(expiry)));
                    cmd.ExecuteNonQuery();
                }

                if (openingStock > 0)
                {
                    // A batch number is needed so the expiry can be tracked per batch.
                    string batch = expiry.HasValue
                        ? "B" + DateTime.Now.ToString("yyyyMMddHHmmss")
                        : null;

                    InsertMovement(c, tx, id, "IN", "Opening Stock", openingStock,
                        batch, expiry, costPrice, null, null, null);
                }

                tx.Commit();
            }
        }

        // Old signature kept so other forms still compile (no expiry).
        public static void InsertProduct(
            string id, string name, string barcode, string categoryId,
            decimal costPrice, decimal sellingPrice, int openingStock)
        {
            InsertProduct(id, name, barcode, categoryId, costPrice, sellingPrice, openingStock, null);
        }

        // Saves the expiry date on the product. Batches already in
        // tbl_stock_movement keep their own expiry date; to bring in stock
        // with a different expiry use Stock In.
        public static void UpdateProduct(
            string id, string name, string barcode, string categoryId,
            decimal costPrice, decimal sellingPrice, DateTime? expiry)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(
                @"UPDATE tbl_product
                  SET product_name = @name,
                      barcode = @barcode,
                      category_id = @cat,
                      cost_price = @cost,
                      selling_price = @price,
                      expiry_date = @expiry
                  WHERE product_id = @id", c))
            {
                cmd.Parameters.Add(P("@id", id));
                cmd.Parameters.Add(P("@name", name.Trim()));
                cmd.Parameters.Add(P("@barcode", TextOrNull(barcode)));
                cmd.Parameters.Add(P("@cat", categoryId));
                cmd.Parameters.Add(P("@cost", costPrice));
                cmd.Parameters.Add(P("@price", sellingPrice));
                cmd.Parameters.Add(P("@expiry", DateOrNull(expiry)));
                cmd.ExecuteNonQuery();
            }
        }

        // Old signature kept so other forms still compile.
        // No expiry argument: the product's existing expiry date is left alone.
        public static void UpdateProduct(
            string id, string name, string barcode, string categoryId,
            decimal costPrice, decimal sellingPrice)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(
                @"UPDATE tbl_product
                  SET product_name = @name,
                      barcode = @barcode,
                      category_id = @cat,
                      cost_price = @cost,
                      selling_price = @price
                  WHERE product_id = @id", c))
            {
                cmd.Parameters.Add(P("@id", id));
                cmd.Parameters.Add(P("@name", name.Trim()));
                cmd.Parameters.Add(P("@barcode", TextOrNull(barcode)));
                cmd.Parameters.Add(P("@cat", categoryId));
                cmd.Parameters.Add(P("@cost", costPrice));
                cmd.Parameters.Add(P("@price", sellingPrice));
                cmd.ExecuteNonQuery();
            }
        }

        // Deletes the product. A product that already has sales or stock
        // movements cannot be deleted (ON DELETE RESTRICT in the schema),
        // so it is set to 'Inactive' instead and disappears from every list.
        // Returns true when it was only deactivated.
        public static bool DeleteProduct(string id)
        {
            using (MySqlConnection c = OpenConnection())
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(
                        "DELETE FROM tbl_product WHERE product_id = @id", c))
                    {
                        cmd.Parameters.Add(P("@id", id));
                        cmd.ExecuteNonQuery();
                    }

                    return false;
                }
                catch (MySqlException ex)
                {
                    // 1451 = a child row (sale item / stock movement) still points here.
                    if (ex.Number != 1451)
                    {
                        throw;
                    }
                }

                using (MySqlCommand cmd = new MySqlCommand(
                    "UPDATE tbl_product SET product_status = 'Inactive' WHERE product_id = @id", c))
                {
                    cmd.Parameters.Add(P("@id", id));
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
        }

        // ------------------------------------------------------------
        // Stock movements
        // ------------------------------------------------------------
        private static void InsertMovement(
            MySqlConnection c, MySqlTransaction tx,
            string productId, string type, string reason, int quantity,
            string batchNo, DateTime? expiry, decimal? unitCost,
            string supplierId, string referenceId, string remarks)
        {
            using (MySqlCommand cmd = new MySqlCommand(
                @"INSERT INTO tbl_stock_movement
                    (product_id, movement_type, reason, quantity, batch_no,
                     expiry_date, unit_cost, supplier_id, reference_id,
                     performed_by, remarks)
                  VALUES
                    (@product, @type, @reason, @qty, @batch,
                     @expiry, @cost, @supplier, @reference,
                     @by, @remarks)", c, tx))
            {
                cmd.Parameters.Add(P("@product", productId));
                cmd.Parameters.Add(P("@type", type));
                cmd.Parameters.Add(P("@reason", reason));
                cmd.Parameters.Add(P("@qty", quantity));
                cmd.Parameters.Add(P("@batch", TextOrNull(batchNo)));
                cmd.Parameters.Add(P("@expiry", DateOrNull(expiry)));
                cmd.Parameters.Add(P("@cost", DecimalOrNull(unitCost)));
                cmd.Parameters.Add(P("@supplier", TextOrNull(supplierId)));
                cmd.Parameters.Add(P("@reference", TextOrNull(referenceId)));
                cmd.Parameters.Add(P("@by", UserOrNull()));
                cmd.Parameters.Add(P("@remarks", TextOrNull(remarks)));
                cmd.ExecuteNonQuery();
            }
        }

        // Current stock, read inside a transaction. "FOR UPDATE" makes two
        // registers selling the same product at once wait for each other.
        private static int GetStock(MySqlConnection c, MySqlTransaction tx, string productId)
        {
            using (MySqlCommand cmd = new MySqlCommand(
                @"SELECT COALESCE(SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END), 0)
                  FROM tbl_stock_movement
                  WHERE product_id = @p
                  FOR UPDATE", c, tx))
            {
                cmd.Parameters.Add(P("@p", productId));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Stock in (isIn = true) or stock out (Damaged / Expired / Adjustment).
        public static void AddMovement(
            string productId, bool isIn, string reason, int quantity,
            string batchNo, DateTime? expiry, decimal? unitCost,
            string supplierId, string remarks)
        {
            // An expiry date without a batch number cannot be tracked per batch
            // (expired screen, first-expired-first-out), so a batch number is made.
            if (isIn && expiry.HasValue && string.IsNullOrWhiteSpace(batchNo))
            {
                batchNo = "B" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            using (MySqlConnection c = OpenConnection())
            using (MySqlTransaction tx = c.BeginTransaction())
            {
                if (!isIn)
                {
                    int stock = GetStock(c, tx, productId);

                    if (quantity > stock)
                    {
                        throw new InvalidOperationException(
                            "Not enough stock. Current stock is " + stock + ".");
                    }
                }

                InsertMovement(c, tx, productId, isIn ? "IN" : "OUT", reason, quantity,
                    batchNo, expiry, unitCost, supplierId, null, remarks);

                // The latest supplier of a restock becomes the product's supplier.
                if (isIn && !string.IsNullOrWhiteSpace(supplierId))
                {
                    using (MySqlCommand cmd = new MySqlCommand(
                        "UPDATE tbl_product SET supplier_id = @s WHERE product_id = @p", c, tx))
                    {
                        cmd.Parameters.Add(P("@s", supplierId.Trim()));
                        cmd.Parameters.Add(P("@p", productId));
                        cmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
        }

        public static DataTable GetHistory(string productId)
        {
            return Query(
                @"SELECT m.created_at AS `Date`,
                         m.movement_type AS `Type`,
                         m.reason AS `Reason`,
                         m.quantity AS `Qty`,
                         m.batch_no AS `Batch`,
                         m.expiry_date AS `Expiry`,
                         m.unit_cost AS `Unit Cost`,
                         a.acc_name AS `By`,
                         m.remarks AS `Remarks`
                  FROM tbl_stock_movement m
                  LEFT JOIN tbl_account a ON a.acc_id = m.performed_by
                  WHERE m.product_id = @p
                  ORDER BY m.created_at DESC, m.movement_id DESC",
                P("@p", productId));
        }

        // ------------------------------------------------------------
        // Sales
        // ------------------------------------------------------------
        // S + yyyyMMdd + - + 4 digits, e.g. S20261007-0001 (sale_id is VARCHAR(20)).
        private static string NextSaleId(MySqlConnection c, MySqlTransaction tx)
        {
            string prefix = "S" + DateTime.Now.ToString("yyyyMMdd") + "-";

            using (MySqlCommand cmd = new MySqlCommand(
                @"SELECT COALESCE(MAX(CAST(SUBSTRING(sale_id, 11) AS UNSIGNED)), 0)
                  FROM tbl_sale
                  WHERE sale_id LIKE @prefix", c, tx))
            {
                cmd.Parameters.Add(P("@prefix", prefix + "%"));
                long last = Convert.ToInt64(cmd.ExecuteScalar());
                return prefix + (last + 1).ToString("D4");
            }
        }

        // Only used to show the next invoice number before the sale is saved.
        public static string PeekNextSaleId()
        {
            using (MySqlConnection c = OpenConnection())
            {
                return NextSaleId(c, null);
            }
        }

        // Saves the sale, its items and the stock OUT movements in ONE
        // transaction: either everything is stored or nothing is.
        // Returns the new sale_id.
        public static string SaveSale(
            List<CartLine> lines, decimal subtotal, decimal discount, decimal tax,
            decimal total, decimal amountPaid, decimal changeDue, string paymentMethod)
        {
            if (lines == null || lines.Count == 0)
            {
                throw new InvalidOperationException("The cart is empty.");
            }

            using (MySqlConnection c = OpenConnection())
            using (MySqlTransaction tx = c.BeginTransaction())
            {
                // 1. Make sure every product still has enough stock.
                foreach (CartLine line in lines)
                {
                    int stock = GetStock(c, tx, line.ProductId);

                    if (line.Quantity > stock)
                    {
                        throw new InvalidOperationException(
                            "Not enough stock for \"" + line.ProductName + "\". Available: " + stock + ".");
                    }
                }

                // 2. Sale header.
                string saleId = NextSaleId(c, tx);

                using (MySqlCommand cmd = new MySqlCommand(
                    @"INSERT INTO tbl_sale
                        (sale_id, cashier_id, subtotal, discount_amount, tax_amount,
                         total_amount, amount_paid, change_due, payment_method, sale_status)
                      VALUES
                        (@id, @cashier, @subtotal, @discount, @tax,
                         @total, @paid, @change, @method, 'Completed')", c, tx))
                {
                    cmd.Parameters.Add(P("@id", saleId));
                    cmd.Parameters.Add(P("@cashier", UserOrNull()));
                    cmd.Parameters.Add(P("@subtotal", subtotal));
                    cmd.Parameters.Add(P("@discount", discount));
                    cmd.Parameters.Add(P("@tax", tax));
                    cmd.Parameters.Add(P("@total", total));
                    cmd.Parameters.Add(P("@paid", amountPaid));
                    cmd.Parameters.Add(P("@change", changeDue));
                    cmd.Parameters.Add(P("@method", paymentMethod));
                    cmd.ExecuteNonQuery();
                }

                // 3. Items + stock out.
                foreach (CartLine line in lines)
                {
                    using (MySqlCommand cmd = new MySqlCommand(
                        @"INSERT INTO tbl_sale_item
                            (sale_id, product_id, quantity, unit_price, line_discount, line_total)
                          VALUES
                            (@sale, @product, @qty, @price, @discount, @total)", c, tx))
                    {
                        cmd.Parameters.Add(P("@sale", saleId));
                        cmd.Parameters.Add(P("@product", line.ProductId));
                        cmd.Parameters.Add(P("@qty", line.Quantity));
                        cmd.Parameters.Add(P("@price", line.UnitPrice));
                        cmd.Parameters.Add(P("@discount", line.LineDiscount));
                        cmd.Parameters.Add(P("@total", line.LineTotal));
                        cmd.ExecuteNonQuery();
                    }

                    DeductStockForSale(c, tx, saleId, line.ProductId, line.Quantity);
                }

                tx.Commit();
                return saleId;
            }
        }

        // First-expired-first-out: take the quantity from the batches that
        // expire first (skipping already expired ones). Whatever is not
        // covered by a batch is booked as a plain OUT movement.
        private static void DeductStockForSale(
            MySqlConnection c, MySqlTransaction tx, string saleId, string productId, int quantity)
        {
            List<object[]> batches = new List<object[]>();

            using (MySqlCommand cmd = new MySqlCommand(
                @"SELECT batch_no,
                         expiry_date,
                         SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END) AS remaining
                  FROM tbl_stock_movement
                  WHERE product_id = @p AND batch_no IS NOT NULL
                  GROUP BY batch_no, expiry_date
                  HAVING remaining > 0
                     AND (expiry_date IS NULL OR expiry_date >= CURDATE())
                  ORDER BY (expiry_date IS NULL), expiry_date, batch_no", c, tx))
            {
                cmd.Parameters.Add(P("@p", productId));

                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        object expiry = null;

                        if (!r.IsDBNull(1))
                        {
                            expiry = r.GetDateTime(1);
                        }

                        batches.Add(new object[]
                        {
                            r.GetString(0),
                            expiry,
                            Convert.ToInt32(r.GetValue(2))
                        });
                    }
                }
            }

            int left = quantity;

            foreach (object[] batch in batches)
            {
                if (left <= 0)
                {
                    break;
                }

                int available = (int)batch[2];
                int take = Math.Min(left, available);

                DateTime? expiry = null;

                if (batch[1] != null)
                {
                    expiry = (DateTime)batch[1];
                }

                InsertMovement(c, tx, productId, "OUT", "Sale", take,
                    (string)batch[0], expiry, null, null, saleId, null);

                left -= take;
            }

            if (left > 0)
            {
                InsertMovement(c, tx, productId, "OUT", "Sale", left,
                    null, null, null, null, saleId, null);
            }
        }

        // ------------------------------------------------------------
        // Dashboard
        // ------------------------------------------------------------
        // The range is inclusive of both days.
        public static DataTable GetSales(DateTime from, DateTime to)
        {
            return Query(
                @"SELECT s.sale_id,
                         (SELECT COALESCE(SUM(i.quantity), 0)
                          FROM tbl_sale_item i
                          WHERE i.sale_id = s.sale_id) AS item_count,
                         s.total_amount,
                         s.created_at
                  FROM tbl_sale s
                  WHERE s.sale_status = 'Completed'
                    AND s.created_at >= @from
                    AND s.created_at < @to
                  ORDER BY s.created_at DESC",
                P("@from", from.Date),
                P("@to", to.Date.AddDays(1)));
        }

        public static DataTable GetSalesByCategory(DateTime from, DateTime to)
        {
            return Query(
                @"SELECT c.category_name, SUM(i.line_total) AS total
                  FROM tbl_sale_item i
                  INNER JOIN tbl_sale s ON s.sale_id = i.sale_id
                  INNER JOIN tbl_product p ON p.product_id = i.product_id
                  INNER JOIN tbl_category c ON c.category_id = p.category_id
                  WHERE s.sale_status = 'Completed'
                    AND s.created_at >= @from
                    AND s.created_at < @to
                  GROUP BY c.category_name
                  ORDER BY total DESC",
                P("@from", from.Date),
                P("@to", to.Date.AddDays(1)));
        }

        public static decimal GetCancelledTotal(DateTime from, DateTime to)
        {
            DataTable dt = Query(
                @"SELECT COALESCE(SUM(total_amount), 0)
                  FROM tbl_sale
                  WHERE sale_status = 'Cancelled'
                    AND created_at >= @from
                    AND created_at < @to",
                P("@from", from.Date),
                P("@to", to.Date.AddDays(1)));

            return Convert.ToDecimal(dt.Rows[0][0]);
        }

        // ------------------------------------------------------------
        // Reports
        // ------------------------------------------------------------

        // Damage report: stock taken out as "Damaged" or "Adjustment".
        // reason = "" shows both. Loss value uses the movement's own unit
        // cost, or the product's cost price when the movement has none.
        public static DataTable GetStockOutReport(DateTime from, DateTime to, string reason, string search)
        {
            return Query(
                @"SELECT m.movement_id,
                         m.created_at,
                         p.product_id,
                         p.product_name,
                         c.category_name,
                         m.reason,
                         m.quantity,
                         COALESCE(m.unit_cost, p.cost_price) AS unit_cost,
                         m.quantity * COALESCE(m.unit_cost, p.cost_price) AS loss,
                         a.acc_name AS reported_by,
                         m.remarks
                  FROM tbl_stock_movement m
                  INNER JOIN tbl_product p ON p.product_id = m.product_id
                  INNER JOIN tbl_category c ON c.category_id = p.category_id
                  LEFT JOIN tbl_account a ON a.acc_id = m.performed_by
                  WHERE m.movement_type = 'OUT'
                    AND m.reason IN ('Damaged', 'Adjustment')
                    AND (@reason = '' OR m.reason = @reason)
                    AND m.created_at >= @from
                    AND m.created_at < @to
                    AND (p.product_id LIKE @s
                         OR p.product_name LIKE @s
                         OR m.remarks LIKE @s)
                  ORDER BY m.created_at DESC, m.movement_id DESC",
                P("@reason", (reason ?? "").Trim()),
                P("@from", from.Date),
                P("@to", to.Date.AddDays(1)),
                P("@s", "%" + (search ?? "").Trim() + "%"));
        }

        // Batches that still have stock and an expiry date, PLUS stock that has
        // no batch but whose product has an expiry date (saved from Manage
        // Product, e.g. on Update).
        // days = -1 : only expired.  days >= 0 : expired + expiring within that many days.
        public static DataTable GetExpiringBatches(int days, string search)
        {
            return Query(
                @"SELECT x.product_id,
                         x.product_name,
                         x.category_name,
                         x.batch_no,
                         x.expiry_date,
                         DATEDIFF(x.expiry_date, CURDATE()) AS days_left,
                         x.remaining,
                         x.remaining * x.cost_price AS stock_value
                  FROM (
                        SELECT p.product_id, p.product_name, c.category_name,
                               p.cost_price, p.product_status,
                               b.batch_no, b.expiry_date, b.remaining
                        FROM (
                              SELECT product_id,
                                     batch_no,
                                     expiry_date,
                                     SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END) AS remaining
                              FROM tbl_stock_movement
                              WHERE expiry_date IS NOT NULL
                              GROUP BY product_id, batch_no, expiry_date
                              HAVING remaining > 0
                             ) b
                        INNER JOIN tbl_product p ON p.product_id = b.product_id
                        INNER JOIN tbl_category c ON c.category_id = p.category_id

                        UNION ALL

                        SELECT p.product_id, p.product_name, c.category_name,
                               p.cost_price, p.product_status,
                               NULL, p.expiry_date, (t.total - t.tracked)
                        FROM tbl_product p
                        INNER JOIN tbl_category c ON c.category_id = p.category_id
                        INNER JOIN (
                              SELECT product_id,
                                     SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END) AS total,
                                     SUM(CASE WHEN expiry_date IS NOT NULL
                                              THEN (CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END)
                                              ELSE 0 END) AS tracked
                              FROM tbl_stock_movement
                              GROUP BY product_id
                             ) t ON t.product_id = p.product_id
                        WHERE p.expiry_date IS NOT NULL
                          AND (t.total - t.tracked) > 0
                       ) x
                  WHERE x.product_status = 'Active'
                    AND x.expiry_date <= DATE_ADD(CURDATE(), INTERVAL @days DAY)
                    AND (x.product_id LIKE @s
                         OR x.product_name LIKE @s
                         OR x.category_name LIKE @s
                         OR x.batch_no LIKE @s)
                  ORDER BY x.expiry_date, x.product_name",
                P("@days", days),
                P("@s", "%" + (search ?? "").Trim() + "%"));
        }

        // Sales report. status = "" shows Completed and Cancelled.
        public static DataTable GetSalesReport(DateTime from, DateTime to, string status, string search)
        {
            return Query(
                @"SELECT s.sale_id,
                         s.created_at,
                         a.acc_name AS cashier,
                         (SELECT COALESCE(SUM(i.quantity), 0)
                          FROM tbl_sale_item i
                          WHERE i.sale_id = s.sale_id) AS items,
                         s.subtotal,
                         s.discount_amount,
                         s.tax_amount,
                         s.total_amount,
                         s.amount_paid,
                         s.change_due,
                         s.payment_method,
                         s.sale_status
                  FROM tbl_sale s
                  LEFT JOIN tbl_account a ON a.acc_id = s.cashier_id
                  WHERE s.created_at >= @from
                    AND s.created_at < @to
                    AND (@status = '' OR s.sale_status = @status)
                    AND (s.sale_id LIKE @s
                         OR a.acc_name LIKE @s
                         OR s.payment_method LIKE @s)
                  ORDER BY s.created_at DESC, s.sale_id DESC",
                P("@from", from.Date),
                P("@to", to.Date.AddDays(1)),
                P("@status", (status ?? "").Trim()),
                P("@s", "%" + (search ?? "").Trim() + "%"));
        }

        public static DataTable GetSaleItems(string saleId)
        {
            return Query(
                @"SELECT i.product_id,
                         p.product_name,
                         i.quantity,
                         i.unit_price,
                         i.line_discount,
                         i.line_total
                  FROM tbl_sale_item i
                  INNER JOIN tbl_product p ON p.product_id = i.product_id
                  WHERE i.sale_id = @id
                  ORDER BY i.sale_item_id",
                P("@id", saleId));
        }

        // Cancels a completed sale and puts the goods back into stock.
        // For every "Sale" stock-out of this sale a matching stock-in
        // ("Sale Cancelled") is written with the same batch and expiry date,
        // then the sale is marked 'Cancelled' (the dashboard already
        // reports cancelled totals). All in ONE transaction.
        public static void CancelSale(string saleId, string remarks)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlTransaction tx = c.BeginTransaction())
            {
                string status;

                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT sale_status FROM tbl_sale WHERE sale_id = @id FOR UPDATE", c, tx))
                {
                    cmd.Parameters.Add(P("@id", saleId));
                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("Sale " + saleId + " was not found.");
                    }

                    status = Convert.ToString(result);
                }

                if (!string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Sale " + saleId + " is already " + status + ".");
                }

                List<object[]> back = new List<object[]>();

                using (MySqlCommand cmd = new MySqlCommand(
                    @"SELECT product_id, quantity, batch_no, expiry_date
                      FROM tbl_stock_movement
                      WHERE reference_id = @id
                        AND movement_type = 'OUT'
                        AND reason = 'Sale'", c, tx))
                {
                    cmd.Parameters.Add(P("@id", saleId));

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string batch = null;
                            object expiry = null;

                            if (!r.IsDBNull(2))
                            {
                                batch = r.GetString(2);
                            }

                            if (!r.IsDBNull(3))
                            {
                                expiry = r.GetDateTime(3);
                            }

                            back.Add(new object[] { r.GetString(0), Convert.ToInt32(r.GetValue(1)), batch, expiry });
                        }
                    }
                }

                foreach (object[] row in back)
                {
                    DateTime? expiry = null;

                    if (row[3] != null)
                    {
                        expiry = (DateTime)row[3];
                    }

                    InsertMovement(c, tx, (string)row[0], "IN", "Sale Cancelled", (int)row[1],
                        (string)row[2], expiry, null, null, saleId, remarks);
                }

                using (MySqlCommand cmd = new MySqlCommand(
                    "UPDATE tbl_sale SET sale_status = 'Cancelled' WHERE sale_id = @id", c, tx))
                {
                    cmd.Parameters.Add(P("@id", saleId));
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
        }

        // ------------------------------------------------------------
        // Supplier management
        // ------------------------------------------------------------
        public static DataTable GetSupplierList(string search)
        {
            return Query(
                @"SELECT supplier_id, supplier_name, contact_person, phone, email, address
                  FROM tbl_supplier
                  WHERE supplier_id LIKE @s OR supplier_name LIKE @s
                     OR contact_person LIKE @s OR phone LIKE @s OR email LIKE @s
                  ORDER BY supplier_name",
                P("@s", "%" + (search ?? "").Trim() + "%"));
        }

        // SUP001, SUP002, ... (supplier_id is VARCHAR(10)).
        public static string NextSupplierId()
        {
            DataTable dt = Query(
                @"SELECT COALESCE(MAX(CAST(SUBSTRING(supplier_id, 4) AS UNSIGNED)), 0)
                  FROM tbl_supplier WHERE supplier_id REGEXP '^SUP[0-9]+$'");

            return "SUP" + (Convert.ToInt64(dt.Rows[0][0]) + 1).ToString("D3");
        }

        public static void InsertSupplier(string name, string contact, string phone, string email, string address)
        {
            string id = NextSupplierId();

            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(
                @"INSERT INTO tbl_supplier (supplier_id, supplier_name, contact_person, phone, email, address)
                  VALUES (@id, @name, @contact, @phone, @email, @address)", c))
            {
                cmd.Parameters.Add(P("@id", id));
                cmd.Parameters.Add(P("@name", name.Trim()));
                cmd.Parameters.Add(P("@contact", TextOrNull(contact)));
                cmd.Parameters.Add(P("@phone", TextOrNull(phone)));
                cmd.Parameters.Add(P("@email", TextOrNull(email)));
                cmd.Parameters.Add(P("@address", TextOrNull(address)));
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSupplier(string id, string name, string contact, string phone, string email, string address)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(
                @"UPDATE tbl_supplier
                  SET supplier_name = @name, contact_person = @contact, phone = @phone,
                      email = @email, address = @address
                  WHERE supplier_id = @id", c))
            {
                cmd.Parameters.Add(P("@id", id));
                cmd.Parameters.Add(P("@name", name.Trim()));
                cmd.Parameters.Add(P("@contact", TextOrNull(contact)));
                cmd.Parameters.Add(P("@phone", TextOrNull(phone)));
                cmd.Parameters.Add(P("@email", TextOrNull(email)));
                cmd.Parameters.Add(P("@address", TextOrNull(address)));
                cmd.ExecuteNonQuery();
            }
        }

        // Products and stock movements keep their rows; the schema sets their
        // supplier to NULL (ON DELETE SET NULL).
        public static void DeleteSupplier(string id)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand("DELETE FROM tbl_supplier WHERE supplier_id = @id", c))
            {
                cmd.Parameters.Add(P("@id", id));
                cmd.ExecuteNonQuery();
            }
        }

        // ------------------------------------------------------------
        // Category management
        // ------------------------------------------------------------
        public static DataTable GetCategoryList()
        {
            return Query(
                @"SELECT c.category_id, c.category_name, COUNT(p.product_id) AS products
                  FROM tbl_category c
                  LEFT JOIN tbl_product p ON p.category_id = c.category_id
                  GROUP BY c.category_id, c.category_name
                  ORDER BY c.category_name");
        }

        // CAT001, CAT002, ...
        public static string NextCategoryId()
        {
            DataTable dt = Query(
                @"SELECT COALESCE(MAX(CAST(SUBSTRING(category_id, 4) AS UNSIGNED)), 0)
                  FROM tbl_category WHERE category_id REGEXP '^CAT[0-9]+$'");

            return "CAT" + (Convert.ToInt64(dt.Rows[0][0]) + 1).ToString("D3");
        }

        public static void InsertCategory(string name)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(
                "INSERT INTO tbl_category (category_id, category_name) VALUES (@id, @name)", c))
            {
                cmd.Parameters.Add(P("@id", NextCategoryId()));
                cmd.Parameters.Add(P("@name", name.Trim()));
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateCategory(string id, string name)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(
                "UPDATE tbl_category SET category_name = @name WHERE category_id = @id", c))
            {
                cmd.Parameters.Add(P("@id", id));
                cmd.Parameters.Add(P("@name", name.Trim()));
                cmd.ExecuteNonQuery();
            }
        }

        // A category that still has products cannot be deleted (ON DELETE RESTRICT).
        public static void DeleteCategory(string id)
        {
            try
            {
                using (MySqlConnection c = OpenConnection())
                using (MySqlCommand cmd = new MySqlCommand("DELETE FROM tbl_category WHERE category_id = @id", c))
                {
                    cmd.Parameters.Add(P("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    throw new InvalidOperationException(
                        "This category still has products. Move or delete them first.");
                }

                throw;
            }
        }

        // ------------------------------------------------------------
        // Low stock
        // ------------------------------------------------------------
        public static DataTable GetLowStock(int level, string search)
        {
            return Query(
                @"SELECT p.product_id,
                         p.product_name,
                         c.category_name,
                         sp.supplier_name,
                         COALESCE(s.current_quantity, 0) AS stock,
                         p.cost_price
                  FROM tbl_product p
                  INNER JOIN tbl_category c ON c.category_id = p.category_id
                  LEFT JOIN tbl_supplier sp ON sp.supplier_id = p.supplier_id
                  LEFT JOIN vw_product_stock s ON s.product_id = p.product_id
                  WHERE p.product_status = 'Active'
                    AND COALESCE(s.current_quantity, 0) <= @level
                    AND (p.product_id LIKE @s OR p.product_name LIKE @s
                         OR c.category_name LIKE @s OR sp.supplier_name LIKE @s)
                  ORDER BY stock, p.product_name",
                P("@level", level),
                P("@s", "%" + (search ?? "").Trim() + "%"));
        }

        // ------------------------------------------------------------
        // Own password
        // ------------------------------------------------------------
        public static string GetPasswordValue(int accountId)
        {
            DataTable dt = Query("SELECT pass_word FROM tbl_account WHERE acc_id = @id", P("@id", accountId));

            return dt.Rows.Count == 0 ? null : Convert.ToString(dt.Rows[0][0]);
        }

        public static void SetPasswordValue(int accountId, string storedValue)
        {
            using (MySqlConnection c = OpenConnection())
            using (MySqlCommand cmd = new MySqlCommand(
                "UPDATE tbl_account SET pass_word = @p WHERE acc_id = @id", c))
            {
                cmd.Parameters.Add(P("@p", storedValue));
                cmd.Parameters.Add(P("@id", accountId));
                cmd.ExecuteNonQuery();
            }
        }
    }
}