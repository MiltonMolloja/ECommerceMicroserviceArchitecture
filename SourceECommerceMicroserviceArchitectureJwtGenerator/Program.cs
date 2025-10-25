using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var secretKey = "molloja-ecommerce-secret-key-super-secure-2025-minimum-32chars";
var key = Encoding.ASCII.GetBytes(secretKey);

var tokenHandler = new JwtSecurityTokenHandler();
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, "Test User"),
        new Claim(ClaimTypes.Email, "test@example.com")
    }),
    Expires = DateTime.UtcNow.AddHours(2),
    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};

var token = tokenHandler.CreateToken(tokenDescriptor);
var tokenString = tokenHandler.WriteToken(token);

Console.WriteLine("JWT Token:");
Console.WriteLine(tokenString);
