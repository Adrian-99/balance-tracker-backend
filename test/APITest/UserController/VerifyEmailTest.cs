using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
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

        private Guid userId;
        private string emailVerificationCode;
        private string unverifiedUserAccessToken;

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(databaseContext);
            var unverifiedUser = (from user in databaseContext.Users
                                 where user.EmailVerificationCode != null
                                 select user).First();
            userId = unverifiedUser.Id;
            emailVerificationCode = unverifiedUser.EmailVerificationCode;
            GetService<IJwtService>().GenerateTokens(unverifiedUser, out unverifiedUserAccessToken, out _);
        }

        [Test]
        public async Task VerifyEmail_WithCorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = emailVerificationCode;

            var response = await TestUtils.SendHttpRequestAsync(httpClient,
                HttpMethod.Put,
                URL,
                unverifiedUserAccessToken,
                verifyEmailDto);

            var user = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(user.EmailVerificationCode);
        }

        [Test]
        public async Task VerifyEmail_WithToLowerCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = emailVerificationCode.ToLower();

            var response = await TestUtils.SendHttpRequestAsync(httpClient,
                HttpMethod.Put,
                URL,
                unverifiedUserAccessToken,
                verifyEmailDto);

            var user = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(emailVerificationCode, user.EmailVerificationCode);
        }

        [Test]
        public async Task VerifyEmail_WithIncorrectCode()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = "someTotallyIncorrectCode123";

            var response = await TestUtils.SendHttpRequestAsync(httpClient,
                HttpMethod.Put,
                URL,
                unverifiedUserAccessToken,
                verifyEmailDto);

            var user = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(emailVerificationCode, user.EmailVerificationCode);
        }

        [Test]
        public async Task VerifyEmail_Unauthorized()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = emailVerificationCode;

            var response = await TestUtils.SendHttpRequestAsync(httpClient, HttpMethod.Put, URL, null, verifyEmailDto);

            var user = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(emailVerificationCode, user.EmailVerificationCode);
        }

        [Test]
        public async Task VerifyEmail_AuthorizedAsSomeoneElse()
        {
            var verifyEmailDto = new VerifyEmailDto();
            verifyEmailDto.EmailVerificationCode = emailVerificationCode;

            string accessToken;
            var someoneElse = (from someUser in databaseContext.Users
                               where someUser.Id != userId
                               select someUser).First();
            GetService<IJwtService>().GenerateTokens(someoneElse, out accessToken, out _);

            var response = await TestUtils.SendHttpRequestAsync(httpClient, HttpMethod.Put, URL, accessToken, verifyEmailDto);

            var user = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(emailVerificationCode, user.EmailVerificationCode);
        }

        private User GetUserById(Guid id)
        {
            return (from user in databaseContext.Users
                   where user.Id == id
                   select user).First();
        }
    }
}
