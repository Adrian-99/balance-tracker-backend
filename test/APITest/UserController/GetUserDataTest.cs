using Application.Dtos.Outgoing;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
    public class GetUserDataTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/data";

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task GetUserData_ForUserWithVerifiedEmail()
        {
            var user = (from u in databaseContext.Users
                        where u.IsEmailVerified
                        select u).First();
            string accessToken;
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, accessToken);
            var responseContent = JsonConvert.DeserializeObject<UserDataDto>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.AreEqual(user.Username, responseContent.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, responseContent.LastUsernameChangeAt);
            Assert.AreEqual(user.Email, responseContent.Email);
            Assert.IsTrue(responseContent.IsEmailVerified);
            Assert.AreEqual(user.FirstName, responseContent.FirstName);
            Assert.AreEqual(user.LastName, responseContent.LastName);
        }

        [Test]
        public async Task GetUserData_ForUserWithUnverifiedEmail()
        {
            var user = (from u in databaseContext.Users
                        where !u.IsEmailVerified
                        select u).First();
            string accessToken;
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, accessToken);
            var responseContent = JsonConvert.DeserializeObject<UserDataDto>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.AreEqual(user.Username, responseContent.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, responseContent.LastUsernameChangeAt);
            Assert.AreEqual(user.Email, responseContent.Email);
            Assert.IsFalse(responseContent.IsEmailVerified);
            Assert.AreEqual(user.FirstName, responseContent.FirstName);
            Assert.AreEqual(user.LastName, responseContent.LastName);
        }

        [Test]
        public async Task GetUserData_Unauthorized()
        {
            var accessToken = "someTotallyWrongAccessToken";

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, accessToken);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
