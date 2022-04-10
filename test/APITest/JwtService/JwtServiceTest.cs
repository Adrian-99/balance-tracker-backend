using Application.Interfaces;
using Infrastructure.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APITest.JwtService
{
    public class JwtServiceTest : AbstractTestClass
    {
        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(databaseContext);
        }

        [Test]
        public void GenerateTokens()
        {
            string accessToken, refreshToken;
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();

            jwtService.GenerateTokens(user, out accessToken, out refreshToken);

            Assert.NotNull(accessToken);
            Assert.NotNull(refreshToken);
        }

        [Test]
        public void ValidateAccessToken_WithValidToken()
        {
            string accessToken;
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out accessToken, out _);

            var result = jwtService.ValidateAccessToken(accessToken);

            Assert.AreEqual(user.Username, result);
        }

        [Test]
        public void ValidateAccessToken_WithInvalidToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out _, out _);

            var result = jwtService.ValidateAccessToken("someTotallyWrongAccessToken");

            Assert.IsNull(result);
        }

        [Test]
        public void ValidateAccessToken_WithOldToken()
        {
            string accessToken1, accessToken2;
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out accessToken1, out _);

            var result1 = jwtService.ValidateAccessToken(accessToken1);
            Assert.AreEqual(user.Username, result1);

            Thread.Sleep(1000);
            jwtService.GenerateTokens(user, out accessToken2, out _);

            var result2 = jwtService.ValidateAccessToken(accessToken2);
            Assert.AreEqual(user.Username, result2);

            var result3 = jwtService.ValidateAccessToken(accessToken1);
            Assert.IsNull(result3);

            var result4 = jwtService.ValidateAccessToken(accessToken2);
            Assert.IsNull(result4);
        }

        [Test]
        public void ValidateRefreshToken_WithValidToken()
        {
            string refreshToken;
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out _, out refreshToken);

            var result = jwtService.ValidateRefreshToken(refreshToken);

            Assert.AreEqual(user.Username, result);
        }

        [Test]
        public void ValidateRefreshToken_WithInvalidToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out _, out _);

            var result = jwtService.ValidateRefreshToken("someTotallyWrongAccessToken");

            Assert.IsNull(result);
        }

        [Test]
        public void ValidateRefreshToken_WithOldToken()
        {
            string refreshToken1, refreshToken2;
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user, out _, out refreshToken1);

            var result1 = jwtService.ValidateRefreshToken(refreshToken1);
            Assert.AreEqual(user.Username, result1);

            Thread.Sleep(1000);
            jwtService.GenerateTokens(user, out _, out refreshToken2);

            var result2 = jwtService.ValidateRefreshToken(refreshToken2);
            Assert.AreEqual(user.Username, result2);

            var result3 = jwtService.ValidateRefreshToken(refreshToken1);
            Assert.IsNull(result3);

            var result4 = jwtService.ValidateRefreshToken(refreshToken2);
            Assert.IsNull(result4);
        }
    }
}
