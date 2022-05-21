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

namespace APITest.UserController
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
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "User2@gmail.com";
            userRegisterDto.Password = "User2!@#";

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
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "User2@gmail.com";
            userRegisterDto.FirstName = "userFirstName";
            userRegisterDto.LastName = "userLastName";
            userRegisterDto.Password = "User2!@#";

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
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "usEr1";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTakenEmail()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "User1@gmail.com";
            userRegisterDto.Password = "User2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithIncorrectUsername()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2!";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTooLongUsername()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "SomeVeryLongUsernameThatItExceedsMaximumAllowedLengthOfUsername";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithIncorrectEmail()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail";
            userRegisterDto.Password = "User2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTooLongFirstName()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "User2@gmail.com";
            userRegisterDto.FirstName = "SomeVeryLongFirstNameThatItExceedsMaximumAllowedLengthOfFirstName";
            userRegisterDto.LastName = "userLastName";
            userRegisterDto.Password = "User2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithTooLongLastName()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "User2@gmail.com";
            userRegisterDto.FirstName = "userFirstName";
            userRegisterDto.LastName = "SomeVeryLongLastNameThatItExceedsMaximumAllowedLengthOfLastName";
            userRegisterDto.Password = "User2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordSameAsUsername()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User2";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordTooShort()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User2!";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordTooLong()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "$omeVeryL0ngPa$$wordThatMeets4llOtherCriteriaExceptTheMaxLength";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutSmallLetter()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "USER2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutBigLetter()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "user2!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutDigit()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User$!@#";

            await AssertRegisterBadRequest(userRegisterDto);
        }

        [Test]
        public async Task Register_WithPasswordWithoutSpecialCharacter()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User2222";

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