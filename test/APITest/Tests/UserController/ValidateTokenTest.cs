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
    public class ValidateTokenTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/validate-token";

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task ValidateToken_WithValidToken()
        {
            var user = databaseContext.Users.First();
            var tokens = GetService<IJwtService>().GenerateTokens(user);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, tokens.AccessToken);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);
        }

        [Test]
        public async Task ValidateToken_WithInvalidToken()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, "someTotallyWrongAccessToken");
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
