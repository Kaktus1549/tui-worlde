SET GLOBAL event_scheduler = ON;
CREATE EVENT IF NOT EXISTS daily_clear_attempts
ON SCHEDULE EVERY 1 DAY
STARTS '2024-11-04 00:00:00'
DO
BEGIN
    DELETE FROM `Wordle`.`AttemptsHistory`;
    DELETE FROM `Wordle`.`Attempts`;
END;