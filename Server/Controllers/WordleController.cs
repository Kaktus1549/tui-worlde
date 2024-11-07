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
        return Ok("User registered.");
    }

    // POST: api/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UsersDTO loginDto)
    {
        // Find the user by username
        var user = await Task.Run(() => _db.Users.FirstOrDefault(u => u.Username == loginDto.Username));

        if (user == null)
        {
            // Return a generic error message to avoid leaking information
            return BadRequest("Invalid username or password.");
        }

        // Verify the password
        if (!_passwordService.VerifyPassword(user.PasswordHash, loginDto.Password))
        {
            return BadRequest("Invalid username or password.");
        }

        // Content for the JWT token, may change in the future
        Dictionary<string, string> jwtClaims = new Dictionary<string, string>
        {
            {"username", user.Username}
        };

        var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress == null)
        {
            throw new Exception("IP address is required");
        }
        string ipAddr = remoteIpAddress.ToString();

        // Generate the JWT token
        string token = _jwt.GenerateJwtToken(_jwtSecret, _issuer, jwtClaims, ipAddr);

        var cookie = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddHours(24)
        };

        // Set the JWT token as a cookie
        Response.Cookies.Append("token", token, cookie);

        return Ok("User logged in.");
    }
    
    // GET: api/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        // Authorization, in future will be moved to middleware
        string token = Request.Cookies["token"] ?? string.Empty;
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("No token provided.");
        }

        var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress == null)
        {
            throw new Exception("IP address is required");
        }
        string ipAddr = remoteIpAddress.ToString();

        if (!_jwt.ValidateJwtToken(token, _jwtSecret, _issuer, ipAddr))
        {
            return BadRequest("Invalid token.");
        }
        // Get the username from the JWT token
        string username = "";
        try{
            username = _jwt.DecodeJWT(token);
        }
        catch{
            return BadRequest("Invalid token.");
        }

        // Find the user by username
        User? user = await Task.Run(() => _db.GetUser(username));

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        return Ok(new { user.NumberOfWins, user.CurrentStreak });
    }

    // POST: api/validate
    [HttpPost("validate")]
        public async Task<IActionResult> GetWord([FromBody] WordDTO wordDto)
    {
        // Authorization, in future will be moved to middleware
        string token = Request.Cookies["token"] ?? string.Empty;
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("No token provided.");
        }

        var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress == null)
        {
            throw new Exception("IP address is required");
        }
        string ipAddr = remoteIpAddress.ToString();

        if (!_jwt.ValidateJwtToken(token, _jwtSecret, _issuer, ipAddr))
        {
            return BadRequest("Invalid token.");
        }
        // Get the username from the JWT token
        string username = "";
        try{
            username = _jwt.DecodeJWT(token);
        }
        catch{
            return BadRequest("Invalid token.");
        }

        // Find the user by username
        User? user = await Task.Run(() => _db.GetUser(username));

        if (user == null)
        {
            return BadRequest("User seems not to exist.");
        }

        if (!_db.AllowedToPlay(user.Id))
        {
            return BadRequest("You have already played today.");
        }

        if (wordDto.Word.Length != 5 || !_db.ValidateWord(wordDto.Word))
        {
            return BadRequest("Invalid word.");
        }

        // Get the word from the database
        var word = _db.SelectWordOfTheDay();

        ResponseDTO response = WordComparer.CompareWords(wordDto.Word, word, user.Id, _db);
        return Ok(response);
    }

    // GET api/check
    // Client will check if user played that day, if played BUT he hasn't used all 5 tries, endpoint will return his history. Otherwise it will return a message that he has already played.
    // Example: User has played 3 times, he will get his history and will be able to play 2 more times.
    // {"history": [{"0_h": "green", "1_e": "yellow", "2_l": "grey", "3_l": "grey", "4_o": "yellow"}, {"0_h": "green", "1_e": "yellow", "2_l": "grey", "3_l": "grey", "4_o": "yellow"}, {"0_h": "green", "1_e": "yellow", "2_l": "grey", "3_l": "grey", "4_o": "yellow"}]}
    // {"message": "You have already played today."}
    [HttpGet("check")]
    public async Task<IActionResult> Check(){
        // Authorization, in future will be moved to middleware
        string token = Request.Cookies["token"] ?? string.Empty;
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("No token provided.");
        }

        var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress == null)
        {
            throw new Exception("IP address is required");
        }
        string ipAddr = remoteIpAddress.ToString();

        if (!_jwt.ValidateJwtToken(token, _jwtSecret, _issuer, ipAddr))
        {
            return BadRequest("Invalid token.");
        }
        // Get the username from the JWT token
        string username = "";
        try{
            username = _jwt.DecodeJWT(token);
        }
        catch{
            return BadRequest("Invalid token.");
        }

        // Find the user by username
        User? user = await Task.Run(() => _db.GetUser(username));

        if (user == null)
        {
            return BadRequest("User seems not to exist.");
        }
        if (!_db.AllowedToPlay(user.Id))
        {
            return Ok(new { message = "You have already played today." });
        }
        
        Dictionary<string, List<string>> history = _db.RetrieveAttemptsHistory(user.Id);
        return Ok(history);
    }
}