using Application.Dtos;
using Infrastructure.Data;
using NUnit.Framework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace APITest.UserController
{
    public class RegisterTest : AbstractControllerTest
    {
        [SetUp]
        public void Setup()
        {
            DataSeeder.SeedUsers(databaseContext);
        }

        [TearDown]
        public void Clean()
        {
            ClearDatabaseContext();
        }

        [Test]
        public async Task Register_WithCorrectAndDataWithoutName()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.Password = "User2!@#";

            var usersCountBefore = databaseContext.Users.Count();

            var response = await TestUtils.PostWithJsonBodyAsync(httpClient, "/api/user/register", userRegisterDto);
            var lastUser = databaseContext.Users.LastOrDefault();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(usersCountBefore + 1, databaseContext.Users.Count());
            Assert.NotNull(lastUser);

            Assert.AreEqual(userRegisterDto.Username, lastUser.Username);
            Assert.AreEqual(userRegisterDto.Email, lastUser.Email);
            Assert.IsNull(lastUser.FirstName);
            Assert.IsNull(lastUser.LastName);
            Assert.IsNotNull(lastUser.PasswordHash);
            Assert.IsNotNull(lastUser.PasswordSalt);
            Assert.IsNotNull(lastUser.EmailVerificationCode);
            Assert.IsNull(lastUser.ResetPasswordCode);
        }

        [Test]
        public async Task Register_WithCorrectDataAndWithName()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2";
            userRegisterDto.Email = "user2@gmail.com";
            userRegisterDto.FirstName = "userFirstName";
            userRegisterDto.LastName = "userLastName";
            userRegisterDto.Password = "User2!@#";

            var usersCountBefore = databaseContext.Users.Count();

            var response = await TestUtils.PostWithJsonBodyAsync(httpClient, "/api/user/register", userRegisterDto);
            var lastUser = databaseContext.Users.LastOrDefault();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(usersCountBefore + 1, databaseContext.Users.Count());
            Assert.NotNull(lastUser);

            Assert.AreEqual(userRegisterDto.Username, lastUser.Username);
            Assert.AreEqual(userRegisterDto.Email, lastUser.Email);
            Assert.AreEqual(userRegisterDto.FirstName, lastUser.FirstName);
            Assert.AreEqual(userRegisterDto.LastName, lastUser.LastName);
            Assert.IsNotNull(lastUser.PasswordHash);
            Assert.IsNotNull(lastUser.PasswordSalt);
            Assert.IsNotNull(lastUser.EmailVerificationCode);
            Assert.IsNull(lastUser.ResetPasswordCode);
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
        public async Task Register_WithIncorrectUsername()
        {
            var userRegisterDto = new UserRegisterDto();
            userRegisterDto.Username = "User2!";
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

            var response = await TestUtils.PostWithJsonBodyAsync(httpClient, "/api/user/register", userRegisterDto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(usersCountBefore, databaseContext.Users.Count());
        }
    }
}