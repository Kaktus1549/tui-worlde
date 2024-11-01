using Microsoft.EntityFrameworkCore;

public class WordleDB : DbContext
{
    public DbSet<User> Users { get; set; }
    private string _serverAddress;
    private string _databaseName;
    private string _username;
    private string _password;

    public WordleDB(string serverAddress, string databaseName, string username, string password)
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
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = $"Server={_serverAddress};Database={_databaseName};User={_username};Password={_password};";
        optionsBuilder.UseMySql(connectionString,
            new MySqlServerVersion(new Version(8, 0, 21)));
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
}