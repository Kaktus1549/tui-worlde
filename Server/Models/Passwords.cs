public class PasswordService
{
    // Hashing the password
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    }

    // Verifying the password
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(providedPassword, hashedPassword);
    }
}