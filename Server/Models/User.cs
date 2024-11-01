public class User{
    public int Id { get; set; }
    public string Username {
        get{ return Username; }
        set{
            if(string.IsNullOrEmpty(value)){
                // In case the username is empty, we throw an exception
                throw new Exception("Username is required");
            }
        }
    }
    public string PasswordHash {
        get{ return PasswordHash; }
        set{
            if(string.IsNullOrEmpty(value)){
                // In case the password is empty, we throw an exception
                throw new Exception("Password is required");
            }
        }
    }
    public int NumberOfWins { get; set; }
    public int CurrentStreak { get; set; }
}