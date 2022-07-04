using Application.Dtos.Ingoing;
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

namespace APITest.Tests.UserController
{
    public class AuthenticateTest : AbstractTestClass
    {
        private static string URL = "/api/user/authenticate";
        private static string USERNAME = "User1";
        private static string EMAIL = "User1@gmail.com";
        private static string PASSWORD = "User1!@#";

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task Authenticate_WithCorrectUsernameAndPassword()
        {
            var authenticateDto = new AuthenticateDto(USERNAME, PASSWORD);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<TokensDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.NotNull(responseContent.Data.AccessToken);
            Assert.NotNull(responseContent.Data.RefreshToken);

            var jwtService = GetService<IJwtService>();
            Assert.AreEqual(USERNAME, jwtService.ValidateAccessToken(responseContent.Data.AccessToken));
            Assert.AreEqual(USERNAME, jwtService.ValidateRefreshToken(responseContent.Data.RefreshToken));
        }

        [Test]
        public async Task Authenticate_WithCorrectEmailAndPassword()
        {
            var authenticateDto = new AuthenticateDto(EMAIL, PASSWORD);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<TokensDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.NotNull(responseContent.Data.AccessToken);
            Assert.NotNull(responseContent.Data.RefreshToken);

            var jwtService = GetService<IJwtService>();
            Assert.AreEqual(USERNAME, jwtService.ValidateAccessToken(responseContent.Data.AccessToken));
            Assert.AreEqual(USERNAME, jwtService.ValidateRefreshToken(responseContent.Data.RefreshToken));
        }

        [Test]
        public async Task Authenticate_WithPasswordToLower()
        {
            var authenticateDto = new AuthenticateDto(USERNAME, PASSWORD.ToLower());

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }

        [Test]
        public async Task Authenticate_WithWrongPassword()
        {
            var authenticateDto = new AuthenticateDto(USERNAME, "Qwerty1@");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }

        [Test]
        public async Task Authenticate_WithWrongUsername()
        {
            var authenticateDto = new AuthenticateDto("randomUser", PASSWORD);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }

        [Test]
        public async Task Authenticate_WithWrongEmail()
        {
            var authenticateDto = new AuthenticateDto("random@gmail.com", PASSWORD);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
