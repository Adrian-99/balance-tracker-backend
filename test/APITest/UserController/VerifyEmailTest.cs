using Domain.Entities;
using Infrastructure.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APITest.UserController
{
    public class VerifyEmailTest : AbstractControllerTest
    {
        private const string URL = "/api/user/email/verify";

        private Guid userId;
        private string emailVerificationCode;

        protected override void PrepareDatabase()
        {
            DataSeeder.SeedUsers(databaseContext);
            var unverifiedUser = (from user in databaseContext.Users
                                 where user.EmailVerificationCode != null
                                 select user).First();
            userId = unverifiedUser.Id;
            emailVerificationCode = unverifiedUser.EmailVerificationCode;
        }

        [Test]
        public async Task VerifyEmail_WithCorrectCode()
        {
            var response = await httpClient.GetAsync($"{URL}/{emailVerificationCode}");

            var unverifiedUser = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(unverifiedUser.EmailVerificationCode);
        }

        [Test]
        public async Task VerifyEmail_WithToLowerCode()
        {
            var response = await httpClient.GetAsync($"{URL}/{emailVerificationCode.ToLower()}");

            var unverifiedUser = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual(emailVerificationCode, unverifiedUser.EmailVerificationCode);
        }

        [Test]
        public async Task VerifyEmail_WithIncorrectCode()
        {
            var response = await httpClient.GetAsync($"{URL}/someTotallyIncorrectCode123");

            var unverifiedUser = GetUserById(userId);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual(emailVerificationCode, unverifiedUser.EmailVerificationCode);
        }

        private User GetUserById(Guid id)
        {
            return (from user in databaseContext.Users
                   where user.Id == id
                   select user).First();
        }
    }
}
