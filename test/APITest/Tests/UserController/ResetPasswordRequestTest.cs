using Application.Dtos.Ingoing;
using Application.Interfaces;
using Application.Utilities;
using Domain.Entities;
using Domain.Interfaces;
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
    public class ResetPasswordRequestTest : AbstractTestClass
    {
        private const string URL = "/api/user/password/reset/request";

        private Mock<IMailService> mailServiceMock;
        private User user;

        public int ApiResponse { get; private set; }

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users
                .Where(u => u.ResetPasswordCode == null && u.ResetPasswordCodeCreatedAt == null)
                .First();
        }

        protected override void PrepareMocks(IServiceCollection services)
        {
            mailServiceMock = new Mock<IMailService>();
            var mailService = services.Single(s => s.ServiceType == typeof(IMailService));
            services.Remove(mailService);
            services.AddScoped(_ => mailServiceMock.Object);
        }

        [Test]
        public async Task ResetPasswordRequest_WithCorrectUsername()
        {
            var resetPasswordRequestDto = new ResetPasswordRequestDto(user.Username);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, resetPasswordRequestDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.NotNull(userAfter.ResetPasswordCode);
            Assert.NotNull(userAfter.ResetPasswordCodeCreatedAt);

            mailServiceMock.Verify(s => s.SendResetPasswordEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once());
        }

        [Test]
        public async Task ResetPasswordRequest_WithCorrectEmail()
        {
            var resetPasswordRequestDto = new ResetPasswordRequestDto(user.Email);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, resetPasswordRequestDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.NotNull(userAfter.ResetPasswordCode);
            Assert.NotNull(userAfter.ResetPasswordCodeCreatedAt);

            mailServiceMock.Verify(s => s.SendResetPasswordEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once());
        }

        [Test]
        public async Task ResetPasswordRequest_WithIncorrectUsername()
        {
            var resetPasswordRequestDto = new ResetPasswordRequestDto("someTotallyWrongUsername");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, resetPasswordRequestDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            mailServiceMock.Verify(s => s.SendResetPasswordEmailAsync(It.IsAny<User>()), Times.Never());
        }

        [Test]
        public async Task ResetPasswordRequest_WithIncorrectEmail()
        {
            var resetPasswordRequestDto = new ResetPasswordRequestDto("someTotallyWrongEmail@domain.com");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, resetPasswordRequestDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            mailServiceMock.Verify(s => s.SendResetPasswordEmailAsync(It.IsAny<User>()), Times.Never());
        }

        [Test]
        public async Task ResetPasswordRequest_ReplacingExistingCode()
        {
            var otherUserBefore = (from user in databaseContext.Users
                                   where user.ResetPasswordCode != null && user.ResetPasswordCodeCreatedAt != null
                                   select user).First();

            var resetPasswordRequestDto = new ResetPasswordRequestDto(otherUserBefore.Username);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, resetPasswordRequestDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var otherUserAfter = TestUtils.GetUserById(databaseContext, otherUserBefore.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.NotNull(otherUserAfter.ResetPasswordCode);
            Assert.NotNull(otherUserAfter.ResetPasswordCodeCreatedAt);
            Assert.AreNotEqual(otherUserBefore.ResetPasswordCode, otherUserAfter.ResetPasswordCode);
            Assert.AreNotEqual(otherUserBefore.ResetPasswordCodeCreatedAt, otherUserAfter.ResetPasswordCodeCreatedAt);

            mailServiceMock.Verify(s => s.SendResetPasswordEmailAsync(It.Is<User>(u => u.Id.Equals(otherUserBefore.Id))), Times.Once());
        }
    }
}
