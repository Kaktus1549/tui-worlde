using Microsoft.EntityFrameworkCore;

public class WordleDB : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<ValidWords> ValidWords { get; set; }
    public DbSet<WordBank> WordBank { get; set; }
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

    public bool ValidateWord(string word)
    {
        return ValidWords.Any(w => w.validWord == word);
    }

    public string SelectWordOfTheDay()
    {
        int wordCount = WordBank.Count();
        if (wordCount == 0)
        {
            throw new Exception("No words in the bank.");
        }

        // Get the current date hash
        int dateHash = DateTime.Now.ToString("yyyy-MM-dd").GetHashCode();
        // Get the index of the word of the day
        int wordIndex = Math.Abs(dateHash) % wordCount;

        return WordBank.Skip(wordIndex).First().guessWord;
    }
}