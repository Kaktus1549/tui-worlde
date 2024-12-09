USE Wordle;
SET GLOBAL event_scheduler = ON;
CREATE EVENT IF NOT EXISTS daily_clear_attemptsHistory
ON SCHEDULE EVERY 1 DAY
STARTS '2024-12-10 00:00:00'
DO
    DELETE FROM `Wordle`.`AttemptsHistory`;
CREATE EVENT IF NOT EXISTS daily_clear_attempts
ON SCHEDULE EVERY 1 DAY
STARTS '2024-12-10 00:00:00'
DO  
    DELETE FROM `Wordle`.`Attempts`;