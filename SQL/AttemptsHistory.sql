CREATE TABLE `Wordle`.`AttemptsHistory`(
	ID int NOT NULL primary key AUTO_INCREMENT,
	GameID int NOT NULL,
    Result JSON NOT NULL,
    TimeStamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GameID) REFERENCES `Wordle`.`Attempts`(ID)
);