using Application.Dtos.Ingoing;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
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
    public class VerifyEmailTest : AbstractTestClass
    {
        private const string URL = "/api/user/email/verify";

        private User user;
        private string unverifiedUserAccessToken;

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = (from user in databaseContext.Users
                                 where user.EmailVerificationCode != null && user.EmailVerificationCodeCreatedAt != null
                                 select user).First();
            GetService<IJwtService>().GenerateTokens(user, out unverifiedUserAccessToken, out _);
        }

        [Test]
        public async Task VerifyEmail_WithCorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, unverifiedUserAccessToken, verifyEmailDto);

            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(userAfter.EmailVerificationCode);
            Assert.IsNull(userAfter.EmailVerificationCodeCreatedAt);
        }

        [Test]
        public async Task VerifyEmail_WithToLowerCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode.ToLower();

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, unverifiedUserAccessToken, verifyEmailDto);

            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);
        }

        [Test]
        public async Task VerifyEmail_WithIncorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = "someTotallyIncorrectCode123";

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, unverifiedUserAccessToken, verifyEmailDto);

            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);
        }

        [Test]
        public async Task VerifyEmail_WithExpiredCode()
        {
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow.AddHours(-1);
            await GetService<IUserRepository>().UpdateAsync(user);

            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, unverifiedUserAccessToken, verifyEmailDto);

            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);
        }

        [Test]
        public async Task VerifyEmail_Unauthorized()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, null, verifyEmailDto);

            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);
        }

        [Test]
        public async Task VerifyEmail_AuthorizedAsSomeoneElse()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = user.EmailVerificationCode;

            string accessToken;
            var someoneElse = (from someUser in databaseContext.Users
                               where someUser.Id != user.Id
                               select someUser).First();
            GetService<IJwtService>().GenerateTokens(someoneElse, out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, accessToken, verifyEmailDto);

            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);
        }
    }
}
