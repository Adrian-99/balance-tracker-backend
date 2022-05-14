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
    public class ValidateTokenTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/validate-token";

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task ValidateToken_WithValidToken()
        {
            string accessToken;
            var user = databaseContext.Users.First();
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, accessToken);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task ValidateToken_WithInvalidToken()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, "someTotallyWrongAccessToken");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
