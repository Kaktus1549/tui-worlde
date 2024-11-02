CREATE TABLE `Wordle`.`WordBank`(
	ID int NOT NULL primary key AUTO_INCREMENT,
	guessWord varchar(255) NOT NULL
);

-- Source of this is: https://github.com/seanpatlan/wordle-words/blob/main/word-bank.csv