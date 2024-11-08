using System.ComponentModel.DataAnnotations;

public class User{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public int NumberOfWins { get; set; }
    public int CurrentStreak { get; set; }
}
public class UsersDTO{
    [Required (ErrorMessage = "Username is required")]
    public required string Username { get; set; }
    [Required (ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}