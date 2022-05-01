using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Domain.Entities;
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

namespace APITest.UserController
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
        public async Task ChangeUserData_WithCorrectDataWithFirstAndLastName()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = "someNewEmail@gmail.com";
            changeUserDataDto.FirstName = "MyFirstName";
            changeUserDataDto.LastName = "MyLastName";

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<OptionalTokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreEqual(changeUserDataDto.Email.ToLower(), userAfter.Email);
            Assert.AreEqual(changeUserDataDto.FirstName, userAfter.FirstName);
            Assert.AreEqual(changeUserDataDto.LastName, userAfter.LastName);
            Assert.AreNotEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.IsNull(GetService<IJwtService>().ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataWithoutFirstAndLastName()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = "someNewEmail@gmail.com";

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<OptionalTokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreEqual(changeUserDataDto.Email.ToLower(), userAfter.Email);
            Assert.IsNull(userAfter.FirstName);
            Assert.IsNull(userAfter.LastName);
            Assert.AreNotEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.IsNull(GetService<IJwtService>().ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataNotChangedUsername()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = user.Username.ToUpper();
            changeUserDataDto.Email = "someNewEmail@gmail.com";

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<OptionalTokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.Null(responseContent.AccessToken);
            Assert.Null(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreEqual(changeUserDataDto.Email.ToLower(), userAfter.Email);
            Assert.IsNull(userAfter.FirstName);
            Assert.IsNull(userAfter.LastName);
            Assert.AreNotEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreNotEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(user.Username, GetService<IJwtService>().ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Once);
        }

        [Test]
        public async Task ChangeUserData_WithCorrectDataWithNotChangedEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = user.Email.ToUpper();

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var responseContent = JsonConvert.DeserializeObject<OptionalTokensDto>(await response.Content.ReadAsStringAsync());
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);

            Assert.NotNull(responseContent.AccessToken);
            Assert.NotNull(responseContent.RefreshToken);
            Assert.AreEqual("success.user.data.emailNotChanged", responseContent.TranslationKey);

            Assert.AreEqual(changeUserDataDto.Username, userAfter.Username);
            Assert.AreEqual(changeUserDataDto.Email.ToLower(), userAfter.Email);
            Assert.IsNull(userAfter.FirstName);
            Assert.IsNull(userAfter.LastName);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.IsNull(GetService<IJwtService>().ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Never);
        }

        [Test]
        public async Task ChangeUserData_Unauthorized()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = "someNewEmail@gmail.com";
            changeUserDataDto.FirstName = "MyFirstName";
            changeUserDataDto.LastName = "MyLastName";

            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, "someTotallyWrongAccessToken", changeUserDataDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            Assert.AreEqual(user.Username, userAfter.Username);
            Assert.AreEqual(user.Email, userAfter.Email);
            Assert.AreEqual(user.FirstName, userAfter.FirstName);
            Assert.AreEqual(user.LastName, userAfter.LastName);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Never);
        }

        [Test]
        public async Task ChangeUserData_WithIncorrectUsername()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "$omeN3wUsername";
            changeUserDataDto.Email = "someNewEmail@gmail.com";

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTooLongUsername()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsernameThatIsWayTooLongAndThereforeCantBeAccepted";
            changeUserDataDto.Email = "someNewEmail@gmail.com";

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTakenUsername()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "USer1";
            changeUserDataDto.Email = "someNewEmail@gmail.com";

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithIncorrectEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = "someNewEmailgmail.com";

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTakenEmail()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = "User1@gmail.com";

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTooLongFirstName()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = "someNewEmail@gmail.com";
            changeUserDataDto.FirstName = "someWayTooLongFirstNameThatItCantBeAccepted";
            changeUserDataDto.LastName = "MyLastName";

            await AssertBadRequestAsync(changeUserDataDto);
        }

        [Test]
        public async Task ChangeUserData_WithTooLongLastName()
        {
            var changeUserDataDto = new ChangeUserDataDto();
            changeUserDataDto.Username = "someNewUsername";
            changeUserDataDto.Email = "someNewEmail@gmail.com";
            changeUserDataDto.FirstName = "MyFirstName";
            changeUserDataDto.LastName = "someWayTooLongLastNameThatItCantBeAccepted";

            await AssertBadRequestAsync(changeUserDataDto);
        }

        private async Task AssertBadRequestAsync(ChangeUserDataDto changeUserDataDto)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Patch, URL, accessToken, changeUserDataDto);
            var userAfter = TestUtils.GetUserById(databaseContext, user.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.AreEqual(user.Username, userAfter.Username);
            Assert.AreEqual(user.Email, userAfter.Email);
            Assert.AreEqual(user.FirstName, userAfter.FirstName);
            Assert.AreEqual(user.LastName, userAfter.LastName);
            Assert.AreEqual(user.EmailVerificationCode, userAfter.EmailVerificationCode);
            Assert.AreEqual(user.EmailVerificationCodeCreatedAt, userAfter.EmailVerificationCodeCreatedAt);

            Assert.AreEqual(user.Username, GetService<IJwtService>().ValidateAccessToken(accessToken));

            mailServiceMock.Verify(m => m.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(user.Id))), Times.Never);
        }
    }
}
