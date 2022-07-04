using Application.Dtos.Outgoing;
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

namespace APITest.Tests.UserController
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
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.IsNull(jwtService.ValidateAccessToken(accessToken));
            Assert.IsNull(jwtService.ValidateRefreshToken(refreshToken));
        }

        [Test]
        public async Task RevokeTokens_WithIncorrectToken()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL, "someTotallyWrongAccessToken");
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
