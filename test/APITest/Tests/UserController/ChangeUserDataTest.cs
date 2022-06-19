using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
    public class ChangeUserDataTest : AbstractTestClass
    {
        private const string URL = "/api/user/data";

        private Mock<IMailService> mailServiceMock;
        private User user;
        private string accessToken;

        public double HttpStatusCodes { get; private set; }

        protected override void PrepareMocks(IServiceCollection services)
        {
            mailServiceMock = new Mock<IMailService>();
            var mailService = services.Single(s => s.ServiceType == typeof(IMailService));
            services.Remove(mailService);
            services.AddScoped(_ => mailServiceMock.Object);
        }

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
            user = (from u in databaseContext.Users
                    where u.FirstName != null && u.LastName != null
                    select u).First();
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataWithFirstAndLastNameWithChangedUsernameAndEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername", "someNewEmail@gmail.com", "MyFirstName", "MyLastName");

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreNotEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(changeUserDataDto.Email, userAfter.Email);
            Assert.IsFalse(userAfter.IsEmailVerified);
            Assert.AreEqual(changeUserDataDto.FirstName, userAfter.FirstName);
            Assert.AreEqual(changeUserDataDto.LastName, userAfter.LastName);
            Assert.AreNotEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            var jwtService = GetService<IJwtService>();
            Assert.IsNull(jwtService.ValidateAccessToken(accessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateRefreshToken(responseContent.RefreshToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataWithoutFirstAndLastNameWithChangedUsernameAndEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername", "someNewEmail@gmail.com", null, null);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreNotEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(changeUserDataDto.Email, userAfter.Email);
            Assert.IsFalse(userAfter.IsEmailVerified);
            Assert.IsNull(userAfter.FirstName);
            Assert.IsNull(userAfter.LastName);
            Assert.AreNotEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            var jwtService = GetService<IJwtService>();
            Assert.IsNull(jwtService.ValidateAccessToken(accessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateRefreshToken(responseContent.RefreshToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataWithUsernameToUpperWithChangedEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto(user.Username.ToUpper(), "someNewEmail@gmail.com", null, null);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreNotEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(changeUserDataDto.Email, userAfter.Email);
            Assert.IsFalse(userAfter.IsEmailVerified);
            Assert.IsNull(userAfter.FirstName);
            Assert.IsNull(userAfter.LastName);
            Assert.AreNotEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            var jwtService = GetService<IJwtService>();
            Assert.IsNull(jwtService.ValidateAccessToken(accessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateRefreshToken(responseContent.RefreshToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataWithNotChangeUsernameWithChangedEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto(user.Username, "someNewEmail@gmail.com", null, null);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(changeUserDataDto.Email, userAfter.Email);
            Assert.IsFalse(userAfter.IsEmailVerified);
            Assert.IsNull(userAfter.FirstName);
            Assert.IsNull(userAfter.LastName);
            Assert.AreNotEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            var jwtService = GetService<IJwtService>();
            Assert.AreEqual(userAfter.Username, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateRefreshToken(responseContent.RefreshToken));
            Assert.IsNull(jwtService.ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataWithChangedUsernameWithNotChangedEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername", user.Email, null, null);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailNotChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreNotEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(changeUserDataDto.Email, userAfter.Email);
            Assert.AreEqual(user.IsEmailVerified, userAfter.IsEmailVerified);
            Assert.IsNull(userAfter.FirstName);
            Assert.IsNull(userAfter.LastName);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            var jwtService = GetService<IJwtService>();
            Assert.IsNull(jwtService.ValidateAccessToken(accessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateAccessToken(responseContent.AccessToken));
            Assert.AreEqual(userAfter.Username, jwtService.ValidateRefreshToken(responseContent.RefreshToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Never);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectNotChangedData()
        {
            var changeUserDataDto = new ChangeUserDataDto(user.Username, user.Email, user.FirstName, user.LastName);

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<TokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.IsNull(responseContent.AccessToken);
            Assert.IsNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailNotChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(changeUserDataDto.Email, userAfter.Email);
            Assert.AreEqual(user.IsEmailVerified, userAfter.IsEmailVerified);
            Assert.AreEqual(changeUserDataDto.FirstName, userAfter.FirstName);
            Assert.AreEqual(changeUserDataDto.LastName, userAfter.LastName);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(user.Username, GetService<IJwtService>().ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Never);
        }

        [Test]
        public async Task ChangeUserData_Unauthorized()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername", "someNewEmail@gmail.com", "MyFirstName", "MyLastName");

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, "someTotallyWrongAccessToken", changeUserDataDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            Assert.AreEqual(user.Username, userAfter.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(user.Email, userAfter.Email);
            Assert.AreEqual(user.IsEmailVerified, userAfter.IsEmailVerified);
            Assert.AreEqual(user.FirstName, userAfter.FirstName);
            Assert.AreEqual(user.LastName, userAfter.LastName);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Never);
        }

        [Test]
        public async Task ChangeUserData_TooFrequentUsernameChange()
        {
            user.LastUsernameChangeAt = DateTime.UtcNow;
            await GetService<IUserRepository>().UpdateAsync(user);

            var changeUserDataDto = new ChangeUserDataDto("someNewUsername", user.Email, user.FirstName, user.FirstName);

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithIncorrectUsername()
        {
            var changeUserDataDto = new ChangeUserDataDto("$omeN3wUsername", "someNewEmail@gmail.com", null, null);

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTooLongUsername()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsernameThatIsWayTooLongAndThereforeCantBeAccepted", "someNewEmail@gmail.com", null, null);

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTakenUsername()
        {
            var changeUserDataDto = new ChangeUserDataDto("USer1", "someNewEmail@gmail.com", null, null);

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithIncorrectEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername", "someNewEmailgmail.com", null, null);

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTakenEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername", "User1@gmail.com", null, null);

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTooLongFirstName()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername",
                                                          "someNewEmail@gmail.com",
                                                          "someWayTooLongFirstNameThatItCantBeAccepted",
                                                          "MyLastName");

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTooLongLastName()
        {
            var changeUserDataDto = new ChangeUserDataDto("someNewUsername",
                                                          "someNewEmail@gmail.com",
                                                          "MyFirstName",
                                                          "someWayTooLongLastNameThatItCantBeAccepted");

            await AssertBadRequestAsync(changeUserDataDto);
        }

        private async Task AssertBadRequestAsync(ChangeUserDataDto changeUserDataDto)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.AreEqual(user.Username, userAfter.Username);
            Assert.AreEqual(user.LastUsernameChangeAt, userAfter.LastUsernameChangeAt);
            Assert.AreEqual(user.Email, userAfter.Email);
            Assert.AreEqual(user.IsEmailVerified, userAfter.IsEmailVerified);
            Assert.AreEqual(user.FirstName, userAfter.FirstName);
            Assert.AreEqual(user.LastName, userAfter.LastName);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(user.Username, GetService<IJwtService>().ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Never);
        }
    }
}
