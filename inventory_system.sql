/*
 inventory_system  -  final schema (fresh install)

 Target   : MariaDB 10.4+ / MySQL 8 (the dump it comes from was MariaDB 10.4.32)
 Use for  : a NEW, empty database.  Existing database -> run
            migrate_finish_schema.sql instead (it keeps your data).

 How the tables fit together
   tbl_account         users (Admin / Cashier / Inventory Staff / Manager)
   tbl_category        product categories
   tbl_supplier        suppliers
   tbl_product         product master data (NO stock column)
   tbl_stock_movement  every stock change: IN (restock, opening, sale cancelled)
                       and OUT (sale, damaged, expired, adjustment).
                       Optional batch_no + expiry_date per movement.
   tbl_sale            sale header   (Completed / Cancelled)
   tbl_sale_item       sale lines
   vw_product_stock    current stock = SUM(IN) - SUM(OUT) per product

 movement.reason values used by the program
   IN : Opening Stock, Restock, Sale Cancelled
   OUT: Sale, Damaged, Expired, Adjustment
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE DATABASE IF NOT EXISTS `inventory_system`
  CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `inventory_system`;

-- ----------------------------
-- tbl_account
-- ----------------------------
CREATE TABLE IF NOT EXISTS `tbl_account` (
  `acc_id`     int NOT NULL,
  `acc_name`   varchar(100) NOT NULL,
  `user_name`  varchar(50)  NOT NULL,
  `acc_type`   enum('Admin','Cashier','Inventory Staff','Manager') NOT NULL,
  `pass_word`  varchar(255) NOT NULL,              -- PBKDF2 hash, ~90 chars
  `created_at` timestamp NOT NULL DEFAULT current_timestamp,
  PRIMARY KEY (`acc_id`),
  UNIQUE KEY `uq_account_username` (`user_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- tbl_category
-- ----------------------------
CREATE TABLE IF NOT EXISTS `tbl_category` (
  `category_id`   varchar(10)  NOT NULL,
  `category_name` varchar(100) NOT NULL,
  PRIMARY KEY (`category_id`),
  UNIQUE KEY `uq_category_name` (`category_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- tbl_supplier
-- ----------------------------
CREATE TABLE IF NOT EXISTS `tbl_supplier` (
  `supplier_id`    varchar(10)  NOT NULL,
  `supplier_name`  varchar(150) NOT NULL,
  `contact_person` varchar(100) DEFAULT NULL,
  `phone`          varchar(20)  DEFAULT NULL,
  `email`          varchar(100) DEFAULT NULL,
  `address`        varchar(255) DEFAULT NULL,
  `created_at`     datetime NOT NULL DEFAULT current_timestamp,
  PRIMARY KEY (`supplier_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- tbl_product
-- ----------------------------
CREATE TABLE IF NOT EXISTS `tbl_product` (
  `product_id`     varchar(10)  NOT NULL,
  `product_name`   varchar(150) NOT NULL,
  `barcode`        varchar(50)  DEFAULT NULL,
  `category_id`    varchar(10)  NOT NULL,
  `supplier_id`    varchar(10)  DEFAULT NULL,
  `cost_price`     decimal(12,2) NOT NULL DEFAULT 0.00,
  `selling_price`  decimal(12,2) NOT NULL DEFAULT 0.00,
  `product_status` varchar(10)  NOT NULL DEFAULT 'Active',   -- Active / Inactive
  `created_at`     datetime NOT NULL DEFAULT current_timestamp,
  PRIMARY KEY (`product_id`),
  UNIQUE KEY `uq_product_barcode` (`barcode`),
  KEY `fk_product_category` (`category_id`),
  KEY `fk_product_supplier` (`supplier_id`),
  CONSTRAINT `fk_product_category` FOREIGN KEY (`category_id`) REFERENCES `tbl_category` (`category_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_product_supplier` FOREIGN KEY (`supplier_id`) REFERENCES `tbl_supplier` (`supplier_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- tbl_sale
-- ----------------------------
CREATE TABLE IF NOT EXISTS `tbl_sale` (
  `sale_id`         varchar(20) NOT NULL,                    -- S20261007-0001
  `cashier_id`      int DEFAULT NULL,
  `subtotal`        decimal(12,2) NOT NULL DEFAULT 0.00,
  `discount_amount` decimal(12,2) NOT NULL DEFAULT 0.00,
  `tax_amount`      decimal(12,2) NOT NULL DEFAULT 0.00,
  `total_amount`    decimal(12,2) NOT NULL DEFAULT 0.00,
  `amount_paid`     decimal(12,2) NOT NULL DEFAULT 0.00,
  `change_due`      decimal(12,2) NOT NULL DEFAULT 0.00,
  `payment_method`  varchar(20) NOT NULL DEFAULT 'Cash',
  `sale_status`     varchar(20) NOT NULL DEFAULT 'Completed', -- Completed / Cancelled
  `created_at`      datetime NOT NULL DEFAULT current_timestamp,
  PRIMARY KEY (`sale_id`),
  KEY `fk_sale_cashier` (`cashier_id`),
  KEY `idx_sale_created` (`created_at`),
  KEY `idx_sale_status` (`sale_status`, `created_at`),
  CONSTRAINT `fk_sale_cashier` FOREIGN KEY (`cashier_id`) REFERENCES `tbl_account` (`acc_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- tbl_sale_item
-- ----------------------------
CREATE TABLE IF NOT EXISTS `tbl_sale_item` (
  `sale_item_id`  bigint unsigned NOT NULL AUTO_INCREMENT,
  `sale_id`       varchar(20) NOT NULL,
  `product_id`    varchar(10) NOT NULL,
  `quantity`      int NOT NULL,
  `unit_price`    decimal(12,2) NOT NULL,
  `line_discount` decimal(12,2) NOT NULL DEFAULT 0.00,
  `line_total`    decimal(12,2) NOT NULL,
  PRIMARY KEY (`sale_item_id`),
  KEY `idx_saleitem_sale` (`sale_id`),
  KEY `idx_saleitem_product` (`product_id`),
  CONSTRAINT `fk_saleitem_product` FOREIGN KEY (`product_id`) REFERENCES `tbl_product` (`product_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_saleitem_sale` FOREIGN KEY (`sale_id`) REFERENCES `tbl_sale` (`sale_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- tbl_stock_movement
-- ----------------------------
CREATE TABLE IF NOT EXISTS `tbl_stock_movement` (
  `movement_id`   bigint unsigned NOT NULL AUTO_INCREMENT,
  `product_id`    varchar(10) NOT NULL,
  `movement_type` enum('IN','OUT') NOT NULL,
  `reason`        varchar(30) NOT NULL,
  `quantity`      int NOT NULL,
  `batch_no`      varchar(50) DEFAULT NULL,
  `expiry_date`   date DEFAULT NULL,
  `unit_cost`     decimal(12,2) DEFAULT NULL,
  `supplier_id`   varchar(10) DEFAULT NULL,
  `reference_id`  varchar(20) DEFAULT NULL,                  -- sale_id for Sale / Sale Cancelled
  `performed_by`  int DEFAULT NULL,
  `remarks`       varchar(255) DEFAULT NULL,
  `created_at`    datetime NOT NULL DEFAULT current_timestamp,
  PRIMARY KEY (`movement_id`),
  KEY `fk_movement_supplier` (`supplier_id`),
  KEY `fk_movement_account` (`performed_by`),
  KEY `idx_movement_product` (`product_id`),
  KEY `idx_movement_expiry` (`expiry_date`),
  KEY `idx_movement_batch` (`product_id`, `batch_no`, `expiry_date`),
  KEY `idx_movement_reference` (`reference_id`),
  KEY `idx_movement_reason` (`reason`, `created_at`),
  CONSTRAINT `fk_movement_account` FOREIGN KEY (`performed_by`) REFERENCES `tbl_account` (`acc_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_movement_product` FOREIGN KEY (`product_id`) REFERENCES `tbl_product` (`product_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_movement_supplier` FOREIGN KEY (`supplier_id`) REFERENCES `tbl_supplier` (`supplier_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Views
-- ----------------------------
-- Current stock per product.
CREATE OR REPLACE VIEW `vw_product_stock` AS
SELECT product_id,
       COALESCE(SUM(CASE WHEN movement_type = 'IN'  THEN quantity ELSE 0 END), 0)
     - COALESCE(SUM(CASE WHEN movement_type = 'OUT' THEN quantity ELSE 0 END), 0)
         AS current_quantity
FROM tbl_stock_movement
GROUP BY product_id;

-- Remaining quantity per batch (only batches that have an expiry date).
-- Same numbers as the Expired Items screen. Informational; the program
-- does not need this view.
CREATE OR REPLACE VIEW `vw_batch_stock` AS
SELECT product_id,
       batch_no,
       expiry_date,
       SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END) AS remaining_quantity
FROM tbl_stock_movement
WHERE expiry_date IS NOT NULL
GROUP BY product_id, batch_no, expiry_date
HAVING remaining_quantity > 0;

-- ----------------------------
-- Starting data
-- ----------------------------
-- Same categories the program creates by itself when tbl_category is empty.
INSERT IGNORE INTO `tbl_category` (`category_id`, `category_name`) VALUES
  ('CAT001', 'Food'),
  ('CAT002', 'Drinks'),
  ('CAT003', 'Grocery'),
  ('CAT004', 'Personal Care'),
  ('CAT005', 'Other');

-- No user is inserted on purpose: passwords are hashed by the program.
-- Start the app, click "Create Account" on the login screen and make the
-- first account type "Admin".

SET FOREIGN_KEY_CHECKS = 1;
