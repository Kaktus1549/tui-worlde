FROM mysql:latest

# Download CSV files
RUN curl -o /var/lib/mysql-files/wordBank.csv https://raw.githubusercontent.com/seanpatlan/wordle-words/refs/heads/main/word-bank.csv \
    && curl -o /var/lib/mysql-files/validWords.csv https://raw.githubusercontent.com/seanpatlan/wordle-words/refs/heads/main/valid-words.csv

# Copy SQL scripts
COPY . /docker-entrypoint-initdb.d/