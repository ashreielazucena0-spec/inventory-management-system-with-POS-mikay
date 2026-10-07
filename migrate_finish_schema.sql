/*
 migrate_finish_schema.sql  -  for your EXISTING inventory_system database

 Keeps all data. Safe to run more than once. Adds only:
   * indexes the new reports use (sales by status, movements by sale / batch / reason)
   * the vw_batch_stock view (informational)

 Needs MariaDB 10.1.4+ (CREATE INDEX IF NOT EXISTS). Your dump header says
 MariaDB 10.4.32, so this works. On MySQL 8 remove the "IF NOT EXISTS" parts
 and run each statement once.

 Nothing in the program depends on this file: the reports work without it,
 they are just faster on large tables with it.
*/

USE `inventory_system`;

CREATE INDEX IF NOT EXISTS `idx_sale_status`        ON `tbl_sale` (`sale_status`, `created_at`);
CREATE INDEX IF NOT EXISTS `idx_movement_batch`     ON `tbl_stock_movement` (`product_id`, `batch_no`, `expiry_date`);
CREATE INDEX IF NOT EXISTS `idx_movement_reference` ON `tbl_stock_movement` (`reference_id`);
CREATE INDEX IF NOT EXISTS `idx_movement_reason`    ON `tbl_stock_movement` (`reason`, `created_at`);

CREATE OR REPLACE VIEW `vw_batch_stock` AS
SELECT product_id,
       batch_no,
       expiry_date,
       SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END) AS remaining_quantity
FROM tbl_stock_movement
WHERE expiry_date IS NOT NULL
GROUP BY product_id, batch_no, expiry_date
HAVING remaining_quantity > 0;
