using Application.Dtos.Ingoing;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
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
    public class ResetPasswordTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/password/reset";
        private User user;

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(databaseContext);
            user = (from user in databaseContext.Users
                    where user.ResetPasswordCode != null && user.ResetPasswordCodeCreatedAt != null
                    select user).First();
        }

        [Test]
        public async Task ResetPassword_WithCorrectData()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "$omeN3wPa$$word";

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, null, resetPasswordDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreNotEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreNotEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.IsNull(userAfter.ResetPasswordCode);
            Assert.IsNull(userAfter.ResetPasswordCodeCreatedAt);
            Assert.AreEqual(userAfter, await GetService<IUserService>().AuthenticateAsync(user.Username, resetPasswordDto.NewPassword));
        }

        [Test]
        public async Task ResetPassword_WithWrongCode()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = "someTotallyWrongCode";
            resetPasswordDto.NewPassword = "$omeN3wPa$$word";

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithExpiredCode()
        {
            user.ResetPasswordCodeCreatedAt = DateTime.UtcNow.AddHours(-1);
            await GetService<IUserRepository>().UpdateAsync(user);

            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "$omeN3wPa$$word";

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithSamePasswordAsCurrent()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "J@n_Kowal$ki123";

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithSamePasswordAsUsername()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "Jan_Kowalski";

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithTooShortPassword()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "J@n1";

            await AssertBadRequest(resetPasswordDto);
        }

        [Test]
        public async Task ResetPassword_WithPasswordWithoutSmallLetter()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "$OMEN3WPA$$WORD";

            await AssertBadRequest(resetPasswordDto);
        }
        
        [Test]
        public async Task ResetPassword_WithPasswordWithoutBigLetter()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "$omen3wpa$$word";

            await AssertBadRequest(resetPasswordDto);
        }
        
        [Test]
        public async Task ResetPassword_WithPasswordWithoutDigit()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "$omeNewPa$$word";

            await AssertBadRequest(resetPasswordDto);
        }
        
        [Test]
        public async Task ResetPassword_WithPasswordWithoutSpecialCharacter()
        {
            var resetPasswordDto = new ResetPasswordDto();
            resetPasswordDto.ResetPasswordCode = user.ResetPasswordCode;
            resetPasswordDto.NewPassword = "SomeN3wPassword";

            await AssertBadRequest(resetPasswordDto);
        }

        private async Task AssertBadRequest(ResetPasswordDto resetPasswordDto)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Put, URL, null, resetPasswordDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(user.PasswordHash, userAfter.PasswordHash);
            Assert.AreEqual(user.PasswordSalt, userAfter.PasswordSalt);
            Assert.AreEqual(user.ResetPasswordCode, userAfter.ResetPasswordCode);
            Assert.AreEqual(user.ResetPasswordCodeCreatedAt, userAfter.ResetPasswordCodeCreatedAt);
        }
    }
}
