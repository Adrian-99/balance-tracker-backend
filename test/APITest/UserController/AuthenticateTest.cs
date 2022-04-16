using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Infrastructure.Data;
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
    public class AuthenticateTest : AbstractTestClass
    {
        private static string URL = "/api/user/authenticate";
        private static string USERNAME = "User1";
        private static string EMAIL = "User1@gmail.com";
        private static string PASSWORD = "User1!@#";

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(databaseContext);
        }

        [Test]
        public async Task Authenticate_WithCorrectUsernameAndPassword()
        {
            var authenticateDto = new AuthenticateDto();
            authenticateDto.UsernameOrEmail = USERNAME;
            authenticateDto.Password = PASSWORD;

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);

            var jwtService = GetService<IJwtService>();
            Assert.AreEqual(USERNAME, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(USERNAME, jwtService.ValidateRefreshToken(responseContent.RefreshToken));
        }

        [Test]
        public async Task Authenticate_WithCorrectEmailAndPassword()
        {
            var authenticateDto = new AuthenticateDto();
            authenticateDto.UsernameOrEmail = EMAIL;
            authenticateDto.Password = PASSWORD;

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);

            var jwtService = GetService<IJwtService>();
            Assert.AreEqual(USERNAME, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(USERNAME, jwtService.ValidateRefreshToken(responseContent.RefreshToken));
        }

        [Test]
        public async Task Authenticate_WithPasswordToLower()
        {
            var authenticateDto = new AuthenticateDto();
            authenticateDto.UsernameOrEmail = USERNAME;
            authenticateDto.Password = PASSWORD.ToLower();

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Authenticate_WithWrongPassword()
        {
            var authenticateDto = new AuthenticateDto();
            authenticateDto.UsernameOrEmail = USERNAME;
            authenticateDto.Password = "Qwerty1@";

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Authenticate_WithWrongUsername()
        {
            var authenticateDto = new AuthenticateDto();
            authenticateDto.UsernameOrEmail = "randomUser";
            authenticateDto.Password = PASSWORD;

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Authenticate_WithWrongEmail()
        {
            var authenticateDto = new AuthenticateDto();
            authenticateDto.UsernameOrEmail = "random@gmail.com";
            authenticateDto.Password = PASSWORD;

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, authenticateDto);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
