using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookSpring.DataLib.DataModels;
using Microsoft.IdentityModel.Tokens;

namespace BookSpring.WebApi.Controllers;

public class JwtHelper(IConfiguration configuration)
{
    public string GetMemberToken(UserModel model)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, model.Name),
            new Claim(ClaimTypes.Role, model.Identity),
            new Claim(ClaimTypes.NameIdentifier,model.Id)
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var securityToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.Now, //notBefore
            expires: DateTime.Now.AddHours(2), //expires
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}