using Application.Interfaces;
using Application.Utilities;
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
            TestDataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task RevokeTokens_WithCorrectToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            var tokens = jwtService.GenerateTokens(user);

            Assert.AreEqual(user.Username, jwtService.ValidateAccessToken(tokens.AccessToken));
            Assert.AreEqual(user.Username, jwtService.ValidateRefreshToken(tokens.RefreshToken));

            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL, tokens.AccessToken);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.IsNull(jwtService.ValidateAccessToken(tokens.AccessToken));
            Assert.IsNull(jwtService.ValidateRefreshToken(tokens.RefreshToken));
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
