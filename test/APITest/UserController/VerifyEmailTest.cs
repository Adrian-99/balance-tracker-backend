using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
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
    public class VerifyEmailTest : AbstractTestClass
    {
        private const string URL = "/api/user/verify-email";

        private User user;
        private string unverifiedUserAccessToken;

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = (from user in databaseContext.Users
                    where !user.IsEmailVerified
                    select user).First();
            GetService<IJwtService>().GenerateTokens(user, out unverifiedUserAccessToken, out _);
        }

        [Test]
        public async Task VerifyEmail_WithCorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserAccessToken, verifyEmailDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);

            Assert.IsNull(userAfter.EmailVerificationCode);
            Assert.IsNull(userAfter.EmailVerificationCodeCreatedAt);

            var jwtService = GetService<IJwtService>();
            Assert.AreEqual(userAfter.Username, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateRefreshToken(responseContent.RefreshToken));
            Assert.IsNull(jwtService.ValidateAccessToken(unverifiedUserAccessToken));
        }

        [Test]
        public async Task VerifyEmail_WithToLowerCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode.ToLower();

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserAccessToken, verifyEmailDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(unverifiedUserAccessToken));
        }

        [Test]
        public async Task VerifyEmail_WithIncorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = "someTotallyIncorrectCode123";

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserAccessToken, verifyEmailDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(unverifiedUserAccessToken));
        }

        [Test]
        public async Task VerifyEmail_WithExpiredCode()
        {
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow.AddHours(-1);
            await GetService<IUserRepository>().UpdateAsync(user);

            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserAccessToken, verifyEmailDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(unverifiedUserAccessToken));
        }

        [Test]
        public async Task VerifyEmail_Unauthorized()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, null, verifyEmailDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);
        }

        [Test]
        public async Task VerifyEmail_WithAlreadyVerifiedEmail()
        {
            user.IsEmailVerified = true;
            await GetService<IUserRepository>().UpdateAsync(user);

            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            string accessToken;
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, verifyEmailDto);

            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(accessToken));
        }
    }
}
