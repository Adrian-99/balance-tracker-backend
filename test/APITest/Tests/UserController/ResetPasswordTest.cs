using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
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

namespace APITest.Tests.UserController
{
    public class ResetPasswordTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/password/reset";
        private User user;

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = (from user in databaseContext.Users
                    where user.ResetPasswordCode != null && user.ResetPasswordCodeCreatedAt != null
                    select user).First();
        }

        [Test]
        public async Task ResetPassword_WithCorrectData()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "$omeN3wPa$$word");

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, null, resetPasswordDto);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreNotEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreNotEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.IsNull(userAfter.ResetPasswordCode);
            Assert.IsNull(userAfter.ResetPasswordCodeCreatedAt);
            Assert.AreEqual(userAfter, await GetService<IUserService>().AuthenticateAsync(user.Username, resetPasswordDto.NewPassword));
        }

        [Test]
        public async Task ResetPassword_WithWrongCode()
        {
            var resetPasswordDto = new ResetPasswordDto("someTotallyWrongCode", "$omeN3wPa$$word");

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithExpiredCode()
        {
            user.ResetPasswordCodeCreatedAt = DateTime.UtcNow.AddHours(-1);
            await GetService<IUserRepository>().UpdateAsync(user);

            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "$omeN3wPa$$word");

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithSamePasswordAsCurrent()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "J@n_Kowal$ki123");

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithSamePasswordAsUsername()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "Jan_Kowalski");

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithTooShortPassword()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "J@n1");

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithTooLongPassword()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode,
                                                        "$omeVeryL0ngPa$$wordThatMeets4llOtherCriteriaExceptTheMaxLength");

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithPasswordWithoutSmallLetter()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "$OMEN3WPA$$WORD");

            await AssertBadRequest(resetPasswordDto);
        }
        
        [Test]
        public async Task ResetPassword_WithPasswordWithoutBigLetter()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "$omen3wpa$$word");

            await AssertBadRequest(resetPasswordDto);
        }
        
        [Test]
        public async Task ResetPassword_WithPasswordWithoutDigit()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "$omeNewPa$$word");

            await AssertBadRequest(resetPasswordDto);
        }
        
        [Test]
        public async Task ResetPassword_WithPasswordWithoutSpecialCharacter()
        {
            var resetPasswordDto = new ResetPasswordDto(user.ResetPasswordCode, "SomeN3wPassword");

            await AssertBadRequest(resetPasswordDto);
        }

        private async Task AssertBadRequest(ResetPasswordDto resetPasswordDto)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, null, resetPasswordDto);
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
