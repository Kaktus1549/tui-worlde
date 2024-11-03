CREATE TABLE `Wordle`.`Attempts`(
	ID int NOT NULL primary key AUTO_INCREMENT,
	UserID varchar(255) NOT NULL,
    NumberOfTries int NOT NULL DEFAULT 0,
	FOREIGN KEY (UserID) REFERENCES `Wordle`.`Users`(ID)
);