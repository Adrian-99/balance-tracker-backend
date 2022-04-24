using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest.UserController
{
    public class RevokeTokensTest : AbstractTestClass
    {
        private static string URL = "/api/user/revoke-tokens";

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task RevokeTokens_WithCorrectToken()
        {
            var user = databaseContext.Users.First();
            string accessToken, refreshToken;
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out accessToken, out refreshToken);

            Assert.AreEqual(user.Username, jwtService.ValidateAccessToken(accessToken));
            Assert.AreEqual(user.Username, jwtService.ValidateRefreshToken(refreshToken));

            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL, accessToken);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsNull(jwtService.ValidateAccessToken(accessToken));
            Assert.IsNull(jwtService.ValidateRefreshToken(refreshToken));
        }

        [Test]
        public async Task RevokeTokens_WithIncorrectToken()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL, "someTotallyWrongAccessToken");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
