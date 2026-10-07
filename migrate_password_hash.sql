-- Run once if the app cannot widen the column itself (no ALTER permission).
-- A hashed password is ~90 chars, so pass_word must hold at least 255.
ALTER TABLE tbl_account MODIFY COLUMN pass_word VARCHAR(255) NOT NULL;
-- Old plain-text passwords keep working: each one is converted to a hash
-- automatically the first time that user logs in.
