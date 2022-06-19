using Application.Dtos.Ingoing;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace APITest.Tests.UserController
{
    public class RegisterTest : AbstractTestClass
    {
        private const string URL = "/api/user/register";

        private Mock<IMailService> mailServiceMock;

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
        }

        [Test]
        public async Task Register_WithCorrectAndDataWithoutName()
        {
            var userRegisterDto = new UserRegisterDto("User2", "User2@gmail.com", null, null, "User2!@#");

            var usersCountBefore = databaseContext.Users.Count();

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, userRegisterDto);
            var lastUser = databaseContext.Users.LastOrDefault();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(usersCountBefore + 1, databaseContext.Users.Count());
            Assert.NotNull(lastUser);

            mailServiceMock.Verify(s => s.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(lastUser.Id))), Times.Once());

            Assert.AreEqual(userRegisterDto.Username, lastUser.Username);
            Assert.NotNull(lastUser.LastUsernameChangeAt);
            Assert.AreEqual(userRegisterDto.Email, lastUser.Email);
            Assert.IsFalse(lastUser.IsEmailVerified);
            Assert.IsNull(lastUser.FirstName);
            Assert.IsNull(lastUser.LastName);
            Assert.IsNotNull(lastUser.PasswordHash);
            Assert.IsNotNull(lastUser.PasswordSalt);
            Assert.IsNotNull(lastUser.EmailVerificationCode);
            Assert.IsNotNull(lastUser.EmailVerificationCodeCreatedAt);
            Assert.IsNull(lastUser.ResetPasswordCode);
            Assert.IsNull(lastUser.ResetPasswordCodeCreatedAt);
        }

        [Test]
        public async Task Register_WithCorrectDataAndWithName()
        {
            var userRegisterDto = new UserRegisterDto("User2", "User2@gmail.com", "userFirstName", "userLastName", "User2!@#");

            var usersCountBefore = databaseContext.Users.Count();

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, userRegisterDto);
            var lastUser = databaseContext.Users.LastOrDefault();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(usersCountBefore + 1, databaseContext.Users.Count());
            Assert.NotNull(lastUser);

            mailServiceMock.Verify(s => s.SendEmailVerificationEmailAsync(It.Is<User>(u => u.Id.Equals(lastUser.Id))), Times.Once());

            Assert.AreEqual(userRegisterDto.Username, lastUser.Username);
            Assert.NotNull(lastUser.LastUsernameChangeAt);
            Assert.AreEqual(userRegisterDto.Email, lastUser.Email);
            Assert.IsFalse(lastUser.IsEmailVerified);
            Assert.AreEqual(userRegisterDto.FirstName, lastUser.FirstName);
            Assert.AreEqual(userRegisterDto.LastName, lastUser.LastName);
            Assert.IsNotNull(lastUser.PasswordHash);
            Assert.IsNotNull(lastUser.PasswordSalt);
            Assert.IsNotNull(lastUser.EmailVerificationCode);
            Assert.IsNotNull(lastUser.EmailVerificationCodeCreatedAt);
            Assert.IsNull(lastUser.ResetPasswordCode);
            Assert.IsNull(lastUser.ResetPasswordCodeCreatedAt);
        }

        [Test]
        public async Task Register_WithTakenUsername()
        {
            var userRegisterDto = new UserRegisterDto("usEr1", "user2@gmail.com", null, null, "User2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTakenEmail()
        {
            var userRegisterDto = new UserRegisterDto("User2", "User1@gmail.com", null, null, "User2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithIncorrectUsername()
        {
            var userRegisterDto = new UserRegisterDto("User2!", "user2@gmail.com", null, null, "User2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTooLongUsername()
        {
            var userRegisterDto = new UserRegisterDto("SomeVeryLongUsernameThatItExceedsMaximumAllowedLengthOfUsername",
                                                      "user2@gmail.com",
                                                      null,
                                                      null,
                                                      "User2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithIncorrectEmail()
        {
            var userRegisterDto = new UserRegisterDto("User2", "user2@gmail", null, null, "User2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTooLongFirstName()
        {
            var userRegisterDto = new UserRegisterDto("User2",
                                                      "User2@gmail.com",
                                                      "SomeVeryLongFirstNameThatItExceedsMaximumAllowedLengthOfFirstName",
                                                      "userLastName",
                                                      "User2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTooLongLastName()
        {
            var userRegisterDto = new UserRegisterDto("User2",
                                                      "User2@gmail.com",
                                                      "userFirstName",
                                                      "SomeVeryLongLastNameThatItExceedsMaximumAllowedLengthOfLastName",
                                                      "User2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordSameAsUsername()
        {
            var userRegisterDto = new UserRegisterDto("User2", "user2@gmail.com", null, null, "User2");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordTooShort()
        {
            var userRegisterDto = new UserRegisterDto("User2", "user2@gmail.com", null, null, "User2!");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordTooLong()
        {
            var userRegisterDto = new UserRegisterDto("User2",
                                                      "user2@gmail.com",
                                                      null,
                                                      null,
                                                      "$omeVeryL0ngPa$$wordThatMeets4llOtherCriteriaExceptTheMaxLength");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutSmallLetter()
        {
            var userRegisterDto = new UserRegisterDto("User2", "user2@gmail.com", null, null, "USER2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutBigLetter()
        {
            var userRegisterDto = new UserRegisterDto("User2", "user2@gmail.com", null, null, "user2!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutDigit()
        {
            var userRegisterDto = new UserRegisterDto("User2", "user2@gmail.com", null, null, "User$!@#");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutSpecialCharacter()
        {
            var userRegisterDto = new UserRegisterDto("User2", "user2@gmail.com", null, null, "User2222");

            await AssertRegisterBadRequest(userRegisterDto);
        }

        private async Task AssertRegisterBadRequest(UserRegisterDto userRegisterDto)
        {
            var usersCountBefore = databaseContext.Users.Count();

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, null, userRegisterDto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(usersCountBefore, databaseContext.Users.Count());

            mailServiceMock.Verify(s => s.SendEmailVerificationEmailAsync(It.IsAny<User>()), Times.Never());
        }
    }
}