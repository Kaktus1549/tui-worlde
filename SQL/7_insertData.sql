USE Wordle;

LOAD DATA INFILE '/var/lib/mysql-files/wordBank.csv'
INTO TABLE WordBank
FIELDS TERMINATED BY '\n'
LINES TERMINATED BY '\n'
(guessWord);  -- Insert each line into the guessWord column

LOAD DATA INFILE '/var/lib/mysql-files/validWords.csv'
INTO TABLE ValidWords
FIELDS TERMINATED BY '\n' 
LINES TERMINATED BY '\n'
(validWord); 