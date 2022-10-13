using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APITest.Tests.JwtService
{
    public class JwtServiceTest : AbstractTestClass
    {
        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public void GenerateTokens()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();

            var tokens = jwtService.GenerateTokens(user);

            Assert.NotNull(tokens.AccessToken);
            Assert.NotNull(tokens.RefreshToken);
        }

        [Test]
        public void ValidateAccessToken_WithValidToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            var tokens = jwtService.GenerateTokens(user);

            var result = jwtService.ValidateAccessToken(tokens.AccessToken);

            Assert.AreEqual(user.Username, result);
        }

        [Test]
        public void ValidateAccessToken_WithInvalidToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user);

            var result = jwtService.ValidateAccessToken("someTotallyWrongAccessToken");

            Assert.IsNull(result);
        }

        [Test]
        public void ValidateAccessToken_WithOldToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            var tokens1 = jwtService.GenerateTokens(user);

            var result1 = jwtService.ValidateAccessToken(tokens1.AccessToken);
            Assert.AreEqual(user.Username, result1);

            Thread.Sleep(1000);
            var tokens2 = jwtService.GenerateTokens(user);

            var result2 = jwtService.ValidateAccessToken(tokens2.AccessToken);
            Assert.AreEqual(user.Username, result2);

            var result3 = jwtService.ValidateAccessToken(tokens1.AccessToken);
            Assert.IsNull(result3);

            var result4 = jwtService.ValidateAccessToken(tokens2.AccessToken);
            Assert.IsNull(result4);
        }

        [Test]
        public void ValidateRefreshToken_WithValidToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            var tokens = jwtService.GenerateTokens(user);

            var result = jwtService.ValidateRefreshToken(tokens.RefreshToken);

            Assert.AreEqual(user.Username, result);
        }

        [Test]
        public void ValidateRefreshToken_WithInvalidToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            jwtService.GenerateTokens(user);

            var result = jwtService.ValidateRefreshToken("someTotallyWrongAccessToken");

            Assert.IsNull(result);
        }

        [Test]
        public void ValidateRefreshToken_WithOldToken()
        {
            var user = databaseContext.Users.First();
            var jwtService = GetService<IJwtService>();
            var tokens1 = jwtService.GenerateTokens(user);

            var result1 = jwtService.ValidateRefreshToken(tokens1.RefreshToken);
            Assert.AreEqual(user.Username, result1);

            Thread.Sleep(1000);
            var tokens2 = jwtService.GenerateTokens(user);

            var result2 = jwtService.ValidateRefreshToken(tokens2.RefreshToken);
            Assert.AreEqual(user.Username, result2);

            var result3 = jwtService.ValidateRefreshToken(tokens1.RefreshToken);
            Assert.IsNull(result3);

            var result4 = jwtService.ValidateRefreshToken(tokens2.RefreshToken);
            Assert.IsNull(result4);
        }
    }
}
