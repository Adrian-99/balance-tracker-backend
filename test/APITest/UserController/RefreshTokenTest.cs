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

namespace APITest.UserController
{
    public class RefreshTokenTest : AbstractTestClass
    {
        private static string URL = "/api/user/refresh-token";

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task RefreshToken_WithValidToken()
        {
            string refreshToken1;
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out _, out refreshToken1);

            var refreshTokenDto = new RefreshTokenDto(refreshToken1);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, refreshTokenDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);

            Assert.AreEqual(user.Username, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(user.Username, jwtService.ValidateRefreshToken(responseContent.RefreshToken));
        }

        [Test]
        public async Task RefreshToken_WithInvalidToken()
        {
            var refreshTokenDto = new RefreshTokenDto("someTotallyWrongRefreshToken");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, refreshTokenDto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task RefreshToken_WithAccessToken()
        {
            string accessToken;
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out accessToken, out _);

            var refreshTokenDto = new RefreshTokenDto(accessToken);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, refreshTokenDto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
