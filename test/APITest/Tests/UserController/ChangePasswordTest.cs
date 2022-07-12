using Application.Dtos.Ingoing;
using Application.Interfaces;
using Application.Utilities;
using Domain.Entities;
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

namespace APITest.Tests.UserController
{
    public class ChangePasswordTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/password/change";
        private static readonly string CURRENT_PASSWORD = "User1!@#";
        private User user;
        private string accessToken;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.First();
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);
        }

        [Test]
        public async Task ChangePassword_WithCorrectData()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "$omeN3wPa$$word");

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changePasswordDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreNotEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreNotEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.IsNull(userAfter.ResetPasswordCode);
            Assert.IsNull(userAfter.ResetPasswordCodeCreatedAt);
            Assert.AreEqual(userAfter, await GetService<IUserService>().AuthenticateAsync(user.Username, changePasswordDto.NewPassword));
        }

        [Test]
        public async Task ChangePassword_Unauthorized()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "$omeN3wPa$$word");

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, null, changePasswordDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.AreEqual(user.ResetPasswordCode, userAfter.ResetPasswordCode);
            Assert.AreEqual(user.ResetPasswordCodeCreatedAt, userAfter.ResetPasswordCodeCreatedAt);
        }

        [Test]
        public async Task ChangePassword_WithWrongCurrentPassword()
        {
            var changePasswordDto = new ChangePasswordDto("thisIsWrongPassword", "$omeN3wPa$$word");

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithSamePasswordAsCurrent()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, CURRENT_PASSWORD);

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithSamePasswordAsUsername()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "User1");

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithTooShortPassword()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "J@n1");

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithTooLongPassword()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD,
                                                          "$omeVeryL0ngPa$$wordThatMeets4llOtherCriteriaExceptTheMaxLength");

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutSmallLetter()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "$OMEN3WPA$$WORD");

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutBigLetter()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "$omen3wpa$$word");

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutDigit()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "$omeNewPa$$word");

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutSpecialCharacter()
        {
            var changePasswordDto = new ChangePasswordDto(CURRENT_PASSWORD, "SomeN3wPassword");

            await AssertBadRequest(changePasswordDto);
        }

        private async Task AssertBadRequest(ChangePasswordDto changePasswordDto)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changePasswordDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.AreEqual(user.ResetPasswordCode, userAfter.ResetPasswordCode);
            Assert.AreEqual(user.ResetPasswordCodeCreatedAt, userAfter.ResetPasswordCodeCreatedAt);
        }
    }
}
