using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Application.Utilities;
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

namespace APITest.Tests.UserController
{
    public class VerifyEmailTest : AbstractTestClass
    {
        private const string URL = "/api/user/verify-email";

        private User user;
        private JwtTokens unverifiedUserTokens;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = (from user in databaseContext.Users
                    where !user.IsEmailVerified
                    select user).First();
            unverifiedUserTokens = GetService<IJwtService>().GenerateTokens(user);
        }

        [Test]
        public async Task VerifyEmail_WithCorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto(user.EmailVerificationCode);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserTokens.AccessToken, verifyEmailDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<TokensDto>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.NotNull(responseContent.Data.AccessToken);
            Assert.NotNull(responseContent.Data.RefreshToken);

            Assert.IsNull(userAfter.EmailVerificationCode);
            Assert.IsNull(userAfter.EmailVerificationCodeCreatedAt);

            var jwtService = GetService<IJwtService>();
            Assert.AreEqual(userAfter.Username, jwtService.ValidateAccessToken(responseContent.Data.AccessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateRefreshToken(responseContent.Data.RefreshToken));
            Assert.IsNull(jwtService.ValidateAccessToken(unverifiedUserTokens.AccessToken));
        }

        [Test]
        public async Task VerifyEmail_WithToLowerCode()
        {
            var verifyEmailDto = new VerifyEmailDto(user.EmailVerificationCode.ToLower());

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserTokens.AccessToken, verifyEmailDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(unverifiedUserTokens.AccessToken));
        }

        [Test]
        public async Task VerifyEmail_WithIncorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto("someTotallyIncorrectCode123");

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserTokens.AccessToken, verifyEmailDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(unverifiedUserTokens.AccessToken));
        }

        [Test]
        public async Task VerifyEmail_WithExpiredCode()
        {
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow.AddHours(-1);
            await GetService<IUserRepository>().UpdateAsync(user);

            var verifyEmailDto = new VerifyEmailDto(user.EmailVerificationCode);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, unverifiedUserTokens.AccessToken, verifyEmailDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(unverifiedUserTokens.AccessToken));
        }

        [Test]
        public async Task VerifyEmail_Unauthorized()
        {
            var verifyEmailDto = new VerifyEmailDto(user.EmailVerificationCode);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, null, verifyEmailDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);
        }

        [Test]
        public async Task VerifyEmail_WithAlreadyVerifiedEmail()
        {
            user.IsEmailVerified = true;
            await GetService<IUserRepository>().UpdateAsync(user);

            var verifyEmailDto = new VerifyEmailDto(user.EmailVerificationCode);

            var tokens = GetService<IJwtService>().GenerateTokens(user);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, tokens.AccessToken, verifyEmailDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(userAfter.Username, GetService<IJwtService>().ValidateAccessToken(tokens.AccessToken));
        }
    }
}
