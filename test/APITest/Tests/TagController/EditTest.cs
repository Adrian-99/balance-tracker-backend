using Application;
using Application.Dtos.Ingoing;
using Application.Interfaces;
using Application.Utilities;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
    public class EditTest : AbstractTestClass
    {
        private static readonly string URL = "api/tag/";

        private User user;
        private JwtTokens tokens;
        private Tag tag;
        private int tagsCountBefore;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<CategoriesLoader>(), GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);
            tag = databaseContext.Tags.Where(t => t.UserId.Equals(user.Id) && t.Name.Equals("tag1")).First();

            tagsCountBefore = databaseContext.Tags.Count();
        }

        [Test]
        public async Task Edit_WithCorrectData()
        {
            var tagDto = new EditTagDto("New tag1");

            await AssertSuccessfulActionAsync(tagDto);
        }

        [Test]
        public async Task Edit_WithNameToUpperCase()
        {
            var tagDto = new EditTagDto("TAG1");

            await AssertSuccessfulActionAsync(tagDto);
        }

        [Test]
        public async Task Edit_ForTagOfAnotherUser()
        {
            tag = await databaseContext.Tags.Where(t => !t.UserId.Equals(user.Id)).FirstAsync();

            var tagDto = new EditTagDto("New tag1");

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + tag.Id, tokens.AccessToken, tagDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.NotFound);
        }

        [Test]
        public async Task Edit_WithTooLongName()
        {
            var tagDto = new EditTagDto("New name of tag1 but it's too long");

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + tag.Id, tokens.AccessToken, tagDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Edit_ForUserWithUnverifiedEmail()
        {
            user = await databaseContext.Users.Where(u => !u.IsEmailVerified).FirstAsync();
            tokens = GetService<IJwtService>().GenerateTokens(user);
            tag = await databaseContext.Tags.Where(t => t.UserId.Equals(user.Id)).FirstAsync();

            var tagDto = new EditTagDto("New tag1");

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + tag.Id, tokens.AccessToken, tagDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task Edit_Unauthorized()
        {
            var tagDto = new EditTagDto("New tag1");

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + tag.Id, "someTotallyWrongAccessToken", tagDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.Unauthorized);
        }

        private async Task AssertSuccessfulActionAsync(EditTagDto tagDto)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + tag.Id, tokens.AccessToken, tagDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore, databaseContext.Tags.Count());

            var editedTag = await databaseContext.Tags.Where(t => t.Id.Equals(tag.Id)).FirstAsync();

            Assert.AreEqual(tagDto.Name, editedTag.Name);
            Assert.AreEqual(user.Id, editedTag.UserId);
        }

        private async Task AssertUnsuccessfulActionAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        {
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore, databaseContext.Tags.Count());

            var editedTag = await databaseContext.Tags.Where(t => t.Id.Equals(tag.Id)).FirstAsync();

            Assert.AreEqual(tag.Name, editedTag.Name);
            Assert.AreEqual(tag.UserId, editedTag.UserId);
        }
    }
}
