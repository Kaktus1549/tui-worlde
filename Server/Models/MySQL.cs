using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

public class WordleDB : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<ValidWords> ValidWords { get; set; }
    public DbSet<WordBank> WordBank { get; set; }
    public DbSet<Attempts> Attempts { get; set; }
    public DbSet<AttemptsHistory> AttemptsHistory { get; set; }
    private string _serverAddress;
    private string _databaseName;
    private string _username;
    private string _password;
    private int _port;

    public WordleDB(string serverAddress, string databaseName, string username, string password, int port = 3306)
    {
        if (string.IsNullOrEmpty(serverAddress))
            throw new Exception("Server address is required");
        if (string.IsNullOrEmpty(databaseName))
            throw new Exception("Database name is required");
        if (string.IsNullOrEmpty(username))
            throw new Exception("Username is required");
        if (string.IsNullOrEmpty(password))
            throw new Exception("Password is required");

        _serverAddress = serverAddress;
        _databaseName = databaseName;
        _username = username;
        _password = password;
        _port = port;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = $"Server={_serverAddress};Port={_port};Database={_databaseName};Uid={_username};Pwd={_password};";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), 
                                mysqlOptions => mysqlOptions.EnableRetryOnFailure());
    }

    public void AddUser(User user)
    {
        Users.Add(user);
        SaveChanges();
    }

    public User GetUser(string username)
    {
        var user = Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            throw new Exception($"User with username '{username}' not found.");
        }
        return user;
    }

    public void UpdateUser(User user)
    {
        Users.Update(user);
        SaveChanges();
    }

    public void DeleteUser(User user)
    {
        Users.Remove(user);
        SaveChanges();
    }

    public void UserWin(int userID)
    {
        var user = Users.FirstOrDefault(u => u.Id == userID);
        if (user == null)
        {
            throw new Exception("User not found.");
        }
        user.NumberOfWins++;
        user.CurrentStreak++;
        UpdateUser(user);
    }

    public bool ValidateWord(string word)
    {
        return ValidWords.Any(w => w.validWord == word);
    }

    private int ComputeStableHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Convert first 4 bytes to an integer for a stable hash
            return BitConverter.ToInt32(hashBytes, 0);
        }
    }
    public string SelectWordOfTheDay()
    {
        int wordCount = WordBank.Count();
        if (wordCount == 0)
        {
            throw new Exception("No words in the bank.");
        }

        // Get the current date hash
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        int dateHash = ComputeStableHash(date);
        // Get the index of the word of the day
        int wordIndex = Math.Abs(dateHash) % wordCount;

        return WordBank.Skip(wordIndex).First().guessWord;
    }

    public bool AllowedToPlay(int userID){
        var attempt = Attempts.FirstOrDefault(a => a.UserID == userID);
        int numberOfTries = attempt?.NumberOfTries ?? 0;
        return numberOfTries < 5;
    }

    public Dictionary<string, List<string>> RetrieveAttemptsHistory(int userID)
    {
        var attempt = Attempts.FirstOrDefault(a => a.UserID == userID);
        if (attempt == null)
        {
            // Return an empty dictionary
            // {"history": []}
            return new Dictionary<string, List<string>> { { "history", new List<string>() } };
        }
        int attemptID = attempt.Id;

        // Get the attempts history, sorted by timestamp
        var attemptsHistory = AttemptsHistory.Where(a => a.AttemptID == attemptID).OrderBy(a => a.TimeStamp).ToList();

        Dictionary<string, List<string>> history = new Dictionary<string, List<string>>();
        foreach (var historyItem in attemptsHistory)
        {
            if (!history.ContainsKey("history"))
            {
                history.Add("history", new List<string>());
            }
            history["history"].Add(historyItem.Result);
        }

        return history;
    }

    public void AddAttempt(int userID)
    {
        var attempt = Attempts.FirstOrDefault(a => a.UserID == userID);
        if (attempt == null)
        {
            Attempts.Add(new Attempts { UserID = userID, NumberOfTries = 1 });
        }
        else
        {
            attempt.NumberOfTries++;
            Attempts.Update(attempt);
        }
        SaveChanges();
    }

    public void AddAttemptHistory(int userID, ResponseDTO result)
    {
        var attempt = Attempts.FirstOrDefault(a => a.UserID == userID);
        if (attempt == null)
        {
            throw new Exception("No attempts found.");
        }
        int attemptID = attempt.Id;

        AttemptsHistory.Add(new AttemptsHistory { AttemptID = attemptID, Result = JsonSerializer.Serialize(result), TimeStamp = DateTime.Now });
        SaveChanges();
    }
}