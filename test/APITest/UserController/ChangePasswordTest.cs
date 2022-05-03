using Application.Dtos.Ingoing;
using Application.Interfaces;
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

namespace APITest.UserController
{
    public class ChangePasswordTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/password/change";
        private static readonly string CURRENT_PASSWORD = "User1!@#";
        private User user;
        private string accessToken;

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.First();
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);
        }

        [Test]
        public async Task ChangePassword_WithCorrectData()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "$omeN3wPa$$word";

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changePasswordDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreNotEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreNotEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.IsNull(userAfter.ResetPasswordCode);
            Assert.IsNull(userAfter.ResetPasswordCodeCreatedAt);
            Assert.AreEqual(userAfter, await GetService<IUserService>().AuthenticateAsync(user.Username, changePasswordDto.NewPassword));
        }

        [Test]
        public async Task ChangePassword_Unauthorized()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "$omeN3wPa$$word";

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, null, changePasswordDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.AreEqual(user.ResetPasswordCode, userAfter.ResetPasswordCode);
            Assert.AreEqual(user.ResetPasswordCodeCreatedAt, userAfter.ResetPasswordCodeCreatedAt);
        }

        [Test]
        public async Task ChangePassword_WithWrongCurrentPassword()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = "thisIsWrongPassword";
            changePasswordDto.NewPassword = "$omeN3wPa$$word";

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithSamePasswordAsCurrent()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = CURRENT_PASSWORD;

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithSamePasswordAsUsername()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "User1";

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithTooShortPassword()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "J@n1";

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithTooLongPassword()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "$omeVeryL0ngPa$$wordThatMeets4llOtherCriteriaExceptTheMaxLength";

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutSmallLetter()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "$OMEN3WPA$$WORD";

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutBigLetter()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "$omen3wpa$$word";

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutDigit()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "$omeNewPa$$word";

            await AssertBadRequest(changePasswordDto);
        }

        [Test]
        public async Task ChangePassword_WithPasswordWithoutSpecialCharacter()
        {
            var changePasswordDto = new ChangePasswordDto();
            changePasswordDto.CurrentPassword = CURRENT_PASSWORD;
            changePasswordDto.NewPassword = "SomeN3wPassword";

            await AssertBadRequest(changePasswordDto);
        }

        private async Task AssertBadRequest(ChangePasswordDto changePasswordDto)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changePasswordDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.AreEqual(user.ResetPasswordCode, userAfter.ResetPasswordCode);
            Assert.AreEqual(user.ResetPasswordCodeCreatedAt, userAfter.ResetPasswordCodeCreatedAt);
        }
    }
}
