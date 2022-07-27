using Application.Dtos;
using Application.Interfaces;
using Application.Utilities;
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

namespace APITest.Tests.EntryController
{
    public class CreateTest : AbstractTestClass
    {
        private static readonly string URL = "api/entry";

        private string accessToken;
        private int entriesCountBefore;
        private int entryTagsCountBefore;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<IConfiguration>(), databaseContext);
            var user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);

            entriesCountBefore = databaseContext.Entries.Count();
            entryTagsCountBefore = databaseContext.EntryTags.Count();
        }

        [Test]
        public async Task Create_WithCorrectDataWithoutTags()
        {
            var entryDto = new EntryDto(DateTime.UtcNow, 15.68M, "New entry", null, "costCategory1", new List<string>());

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, accessToken, entryDto);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore + 1, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());

            var lastEntry = databaseContext.Entries
                .Include(e => e.Category)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Last();
            Assert.AreEqual(entryDto.Date, lastEntry.Date);
            Assert.AreEqual(entryDto.Value, lastEntry.Value);
            Assert.AreEqual(entryDto.Name, lastEntry.Name);
            Assert.AreEqual(entryDto.Description, lastEntry.Description);
            Assert.AreEqual(entryDto.CategoryKeyword, lastEntry.Category.Keyword);
            Assert.AreEqual(0, lastEntry.EntryTags.Count);
        }
        
        [Test]
        public async Task Create_WithCorrectDataWithTags()
        {
            var tagNames = new List<string>();
            tagNames.Add("tag1");
            tagNames.Add("secondTag");
            var entryDto = new EntryDto(DateTime.UtcNow, 15.68M, "New entry", null, "costCategory1", tagNames);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, accessToken, entryDto);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore + 1, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore + tagNames.Count, databaseContext.EntryTags.Count());

            var lastEntry = databaseContext.Entries
                .Include(e => e.Category)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Last();
            Assert.AreEqual(entryDto.Date, lastEntry.Date);
            Assert.AreEqual(entryDto.Value, lastEntry.Value);
            Assert.AreEqual(entryDto.Name, lastEntry.Name);
            Assert.AreEqual(entryDto.Description, lastEntry.Description);
            Assert.AreEqual(entryDto.CategoryKeyword, lastEntry.Category.Keyword);
            Assert.AreEqual(tagNames.Count, lastEntry.EntryTags.Count);
            Assert.AreEqual(tagNames[0], lastEntry.EntryTags.ToList()[0].Tag.Name);
            Assert.AreEqual(tagNames[1], lastEntry.EntryTags.ToList()[1].Tag.Name);
        }

        [Test]
        public async Task Create_WithInvalidCategory()
        {
            var entryDto = new EntryDto(DateTime.UtcNow, 15.68M, "New entry", null, "NonExistantCategory", new List<string>());

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, accessToken, entryDto);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
        }

        [Test]
        public async Task Create_WithInvalidTag()
        {
            var tagNames = new List<string>();
            tagNames.Add("tag1");
            tagNames.Add("tag of another user");
            var entryDto = new EntryDto(DateTime.UtcNow, 15.68M, "New entry", null, "costCategory1", tagNames);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, accessToken, entryDto);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
        }

        [Test]
        public async Task Create_Unauthorized()
        {
            var entryDto = new EntryDto(DateTime.UtcNow, 15.68M, "New entry", null, "costCategory1", new List<string>());

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, "someTotallyWrongAccessToken", entryDto);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
        }

        [Test]
        public async Task Create_ForUserWithUnverifiedEmail()
        {
            var user = databaseContext.Users.Where(u => !u.IsEmailVerified).First();
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);

            var entryDto = new EntryDto(DateTime.UtcNow, 15.68M, "New entry", null, "costCategory1", new List<string>());

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, accessToken, entryDto);
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
        }
    }
}
