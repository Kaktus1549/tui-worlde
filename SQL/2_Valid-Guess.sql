CREATE TABLE `Wordle`.`ValidWords`(
	ID int NOT NULL primary key AUTO_INCREMENT,
	validWord varchar(255) NOT NULL
);

-- Source of this is: https://github.com/seanpatlan/wordle-words/blob/main/valid-words.csv