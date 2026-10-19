using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Ssal.Application.Portal;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public class GeneradorTokenJwtPortal(IConfiguration config) : IGeneradorTokenPortal
{
    private const string RolPortal = "PORTAL";

    public string GenerarPara(PortalUsuario cuenta)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, cuenta.IdPortalUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, cuenta.Email),
            new Claim(ClaimTypes.Role, RolPortal),
            new Claim("nombre", cuenta.Cliente.Nombre),
            new Claim("apellido", cuenta.Cliente.Apellido),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
