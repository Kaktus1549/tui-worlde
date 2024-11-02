using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class apiController : ControllerBase
{
    private WordleDB _db;
    private PasswordService _passwordService;
    private string _jwtSecret;
    private string _issuer;
    private JWT _jwt;

    public apiController(WordleDB db,  Dictionary<string, string> configuration)
    {
        _db = db;
        _passwordService = new PasswordService();
        _jwtSecret = configuration["jwtSecret"] ?? throw new Exception("JWT secret is required");
        _issuer = configuration["issuer"] ?? "wordle.kaktusgame.eu";
        _jwt = new JWT();
    }

    // POST: api/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UsersDTO registerDto)
    {
        if (_db.Users.Any(u => u.Username == registerDto.Username))
        {
            return BadRequest("Username is already taken.");
        }

        // Hash the password
        var hashedPassword = _passwordService.HashPassword(registerDto.Password);

        // Create a new user
        var user = new User
        {
            Username = registerDto.Username,
            PasswordHash = hashedPassword,
            NumberOfWins = 0,
            CurrentStreak = 0
        };

        // Add the user to the database
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Redirect to the login endpoint
        return Redirect("login");
    }

    // POST: api/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UsersDTO loginDto)
    {
        // Find the user by username
        var user = await Task.Run(() => _db.Users.FirstOrDefault(u => u.Username == loginDto.Username));

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        // Verify the password
        if (!_passwordService.VerifyPassword(user.PasswordHash, loginDto.Password))
        {
            return BadRequest("Invalid password.");
        }

        // Content for the JWT token, may change in the future
        Dictionary<string, string> jwtClaims = new Dictionary<string, string>
        {
            {"username", user.Username}
        };

        // Generate the JWT token
        string token = _jwt.GenerateJwtToken(_jwtSecret, _issuer, jwtClaims);

        var cookie = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddHours(24)
        };

        // Set the JWT token as a cookie
        Response.Cookies.Append("token", token, cookie);

        return Ok();
    }
    
}
