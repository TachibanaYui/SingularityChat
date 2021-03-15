using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Services
{
    public class JwtTokenServiceBuilder
    {
        public SigningCredentials SigningCredentials { get; set; }
        public SecurityKey Key { get; set; }
        public JwtSecurityTokenHandler TokenHandler { get; set; }
        public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256;
        public byte[] PrivateKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan TokenLifeSpan { get; set; } = TimeSpan.FromHours(1);

        public JwtTokenServiceBuilder WithPrivateKey(string key)
        {
            SigningCredentials = null;
            PrivateKey = Encoding.UTF8.GetBytes(key);
            return this;
        }

        public JwtTokenServiceBuilder WithIssuer(string issuer)
        {
            Issuer = issuer;
            return this;
        }

        public JwtTokenServiceBuilder WithAudience(string audience)
        {
            Audience = audience;
            return this;
        }

        // TODO: Add more methods

        public JwtTokenService Build()
        {
            return JwtTokenService.FromBuider(this);
        }

    }
}
