using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    internal class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenValidMinutes { get; set; }
        public int RefreshTokenValidMinutes { get; set; }

        public byte[] KeyBytes { get => Encoding.UTF8.GetBytes(Key); }

        public static JwtSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("Jwt")
                .Get<JwtSettings>();
        }
    }
}
