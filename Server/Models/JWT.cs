using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public class JWT{
    public string GenerateJwtToken(string secretKey, string issuer, Dictionary<string, string> data, string aud)
    {
        var keyBytes = Convert.FromBase64String(secretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        const int expireMinutes = 1440;

        var claims = new List<Claim>();

        foreach (var item in data)
        {
            claims.Add(new Claim(item.Key, item.Value));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: aud,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
    public bool ValidateJwtToken(string token, string secretKey, string issuer, string aud)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Convert.FromBase64String(secretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = aud,
            ValidateLifetime = true,
            IssuerSigningKey = securityKey
        };

        try
        {
            SecurityToken validatedToken;
            tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public string DecodeJWT(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jsonToken == null){
            throw new Exception("Invalid JWT token.");
        }
        return jsonToken.Claims.First(claim => claim.Type == "username").Value;
    }
}