using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
    public class ResetEmailVerificationCodeTest : AbstractTestClass
    {
        private const string URL = "/api/user/verify-email/reset-code";

        private Mock<IMailService> mailServiceMock;

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        protected override void PrepareMocks(IServiceCollection services)
        {
            mailServiceMock = new Mock<IMailService>();
            var mailService = services.Single(s => s.ServiceType == typeof(IMailService));
            services.Remove(mailService);
            services.AddScoped(_ => mailServiceMock.Object);
        }

        [Test]
        public async Task ResetEmailVerificationCode_ForUserWithUnverifiedEmail()
        {
            string accessToken;
            var userWithUnverifiedEmail = (from user in databaseContext.Users
                                       where !user.IsEmailVerified
                                       select user).First();
            GetService<IJwtService>().GenerateTokens(userWithUnverifiedEmail, out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, accessToken);
            var userAfter = TestUtils.GetUserById(databaseContext, userWithUnverifiedEmail.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreNotEqual(userWithUnverifiedEmail.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(userWithUnverifiedEmail.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id == userWithUnverifiedEmail.Id)), Times.Once);
        }

        [Test]
        public async Task ResetEmailVerificationCode_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, "someTotallyWrongAccessToken");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task ResetEmailVerificationCode_ForUserWithVerifiedEmail()
        {
            string accessToken;
            var userWithVerifiedEmail = (from user in databaseContext.Users
                                         where user.IsEmailVerified
                                         select user).First();
            GetService<IJwtService>().GenerateTokens(userWithVerifiedEmail, out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, accessToken);
            var userAfter = TestUtils.GetUserById(databaseContext, userWithVerifiedEmail.Id);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            Assert.AreEqual(userWithVerifiedEmail.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(userWithVerifiedEmail.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id == userWithVerifiedEmail.Id)), Times.Never);
        }
    }
}
