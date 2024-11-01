CREATE TABLE `Wordle`.Users(
	ID int NOT NULL primary key AUTO_INCREMENT,
	Username varchar(255) NOT NULL,
	PasswordHash varchar(255) NOT NULL,
	NumberOfWins int NOT NULL DEFAULT 0,
	CurrentStreak int NOT NULL DEFAULT 0
);