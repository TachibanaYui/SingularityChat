using Microsoft.IdentityModel.Tokens;
using SingularityChat.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Services
{
    public class JwtTokenService
    {
        public SigningCredentials SigningCredentials { get; set; }
        public SecurityKey Key => SigningCredentials.Key;
        public string Algorithm => SigningCredentials.Algorithm;
        public JwtSecurityTokenHandler TokenHandler { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan TokenLifeSpan { get; set; }

        public static JwtTokenService FromBuider(JwtTokenServiceBuilder builder) 
        {
            return new JwtTokenService()
            {
                SigningCredentials = builder.SigningCredentials ?? new SigningCredentials(builder.Key ?? new SymmetricSecurityKey(builder.PrivateKey), builder.Algorithm),
                TokenHandler = builder.TokenHandler ?? new JwtSecurityTokenHandler(),
                Issuer = builder.Issuer,
                Audience = builder.Audience,
                TokenLifeSpan = builder.TokenLifeSpan
            };
        }

        protected JwtTokenService()
        {

        }


        public string GenerateToken(User user)
        {
            var token = new JwtSecurityToken(
                    Issuer,
                    Audience,
                    new List<Claim>() { new Claim("UserId", user.Id) }, 
                    DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), // Todo
                    DateTime.UtcNow.Add(TokenLifeSpan),
                    SigningCredentials
                );
            return TokenHandler.WriteToken(token);
        }

        public IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            var jwtToken = TokenHandler.ReadJwtToken(token);
            // Check validity

            return jwtToken.Claims;
        }
    }
}
