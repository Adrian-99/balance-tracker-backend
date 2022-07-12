using Application.Dtos.Outgoing;
using Application.Interfaces;
using Application.Utilities;
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

namespace APITest.Tests.UserController
{
    public class GetUserDataTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/data";

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
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
            var responseContent = await GetResponseContentAsync<ApiResponse<UserDataDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(user.Username, responseContent.Data.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, responseContent.Data.LastUsernameChangeAt);
            Assert.AreEqual(user.Email, responseContent.Data.Email);
            Assert.IsTrue(responseContent.Data.IsEmailVerified);
            Assert.AreEqual(user.FirstName, responseContent.Data.FirstName);
            Assert.AreEqual(user.LastName, responseContent.Data.LastName);
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
            var responseContent = await GetResponseContentAsync<ApiResponse<UserDataDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(user.Username, responseContent.Data.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, responseContent.Data.LastUsernameChangeAt);
            Assert.AreEqual(user.Email, responseContent.Data.Email);
            Assert.IsFalse(responseContent.Data.IsEmailVerified);
            Assert.AreEqual(user.FirstName, responseContent.Data.FirstName);
            Assert.AreEqual(user.LastName, responseContent.Data.LastName);
        }

        [Test]
        public async Task GetUserData_Unauthorized()
        {
            var accessToken = "someTotallyWrongAccessToken";

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, accessToken);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
