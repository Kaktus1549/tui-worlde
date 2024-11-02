using Microsoft.EntityFrameworkCore;

public class WordleDB : DbContext
{
    public DbSet<User> Users { get; set; }
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
}