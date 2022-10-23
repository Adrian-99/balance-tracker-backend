using Application;
using Application.Dtos.Ingoing;
using Application.Interfaces;
using Application.Utilities;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests.TagController
{
    public class CreateTest : AbstractTestClass
    {
        private static readonly string URL = "api/tag";

        private User user;
        private JwtTokens tokens;
        private int tagsCountBefore;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<CategoriesLoader>(), GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);
            tagsCountBefore = databaseContext.Tags.Count();
        }

        [Test]
        public async Task Create_WithCorrectData()
        {
            var tagDto = new EditTagDto("new tag");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, tagDto);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore + 1, databaseContext.Tags.Count());

            var lastTag = databaseContext.Tags.Last();
            Assert.AreEqual(tagDto.Name, lastTag.Name);
            Assert.AreEqual(user.Id, lastTag.UserId);
        }

        [Test]
        public async Task Create_WithExistingTagName()
        {
            var tagDto = new EditTagDto("Tag1");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, tagDto);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore, databaseContext.Tags.Count());
        }

        [Test]
        public async Task Create_WithTooLongTagName()
        {
            var tagDto = new EditTagDto("TagWithVeryLongNameThatExceedsLimit");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, tagDto);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore, databaseContext.Tags.Count());
        }

        [Test]
        public async Task Create_Unauthorized()
        {
            var tagDto = new EditTagDto("new tag");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, "totallyWrongAccessToken", tagDto);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore, databaseContext.Tags.Count());
        }

        [Test]
        public async Task Create_ForUserWithUnverifiedEmail()
        {
            user = databaseContext.Users.Where(u => !u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);

            var tagDto = new EditTagDto("new tag");

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, tagDto);
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore, databaseContext.Tags.Count());
        }
    }
}
