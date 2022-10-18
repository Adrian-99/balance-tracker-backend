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

namespace APITest.Tests.EntryController
{
    internal class EditTest : AbstractTestClass
    {
        private static readonly string URL = "api/entry/";

        private User user;
        private JwtTokens tokens;
        private Entry entry;
        private int entriesCountBefore;
        private int entryTagsCountBefore;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<CategoriesLoader>(), GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);
            entry = databaseContext.Entries.Where(e => e.UserId.Equals(user.Id) && e.Name.Equals("Bills")).First();

            entriesCountBefore = databaseContext.Entries.Count();
            entryTagsCountBefore = databaseContext.EntryTags.Count();
        }

        [Test]
        public async Task Edit_WithCorrectDataWithNotChangedTags()
        {
            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertSuccessfulActionAsync(response, entryDto);
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
        }

        [Test]
        public async Task Edit_WithCorrectDataWithRemovedDescription()
        {
            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", null, "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertSuccessfulActionAsync(response, entryDto);
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
        }

        [Test]
        public async Task Edit_WithCorrectDataWithRemovedTags()
        {
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "costCategory1", new List<string>());

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertSuccessfulActionAsync(response, entryDto);
            Assert.AreEqual(entryTagsCountBefore - 2, databaseContext.EntryTags.Count());
        }

        [Test]
        public async Task Edit_WithCorrectDataWithReplacedTag()
        {
            var tags = new List<string>() { "tag1", "secondTag" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertSuccessfulActionAsync(response, entryDto);
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
        }

        [Test]
        public async Task Edit_ForEntryOfAnotherUser()
        {
            entry = await databaseContext.Entries
                .Where(e => !e.UserId.Equals(user.Id))
                .Include(e => e.Category)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .FirstAsync();
            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.NotFound);
        }

        [Test]
        public async Task Edit_WithTooLongDescription()
        {
            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry that is a little bit longer than the limit of length for entry description", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Edit_WithIncorrectCategory()
        {
            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "nonexistantCategory", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Edit_WithTooLongName()
        {
            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name that is too long to be accepted", "New description of the entry", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Edit_WithTagOfAnotherUser()
        {
            var tags = new List<string>() { "tag1", "Tag number 3", "tag of another user" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Edit_ForUserWithUnverifiedEmail()
        {
            user = await databaseContext.Users.Where(u => !u.IsEmailVerified).FirstAsync();
            tokens = GetService<IJwtService>().GenerateTokens(user);

            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, tokens.AccessToken, entryDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task Edit_Unauthorized()
        {
            var tags = new List<string>() { "tag1", "Tag number 3" };
            var entryDto = new EditEntryDto(DateTime.UtcNow, 54.35M, "Changed entry name", "New description of the entry", "costCategory1", tags);

            var response = await SendHttpRequestAsync(HttpMethod.Put, URL + entry.Id, "someTotallyWrongAccessToken", entryDto);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.Unauthorized);
        }

        private async Task AssertSuccessfulActionAsync(HttpResponseMessage response, EditEntryDto entryDto)
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());

            var editedEntry = await databaseContext.Entries
                .Where(e => e.Id.Equals(entry.Id))
                .Include(e => e.Category)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .FirstAsync();

            Assert.AreEqual(entryDto.Date, editedEntry.Date);
            Assert.AreEqual(entryDto.Value, editedEntry.Value);
            Assert.AreEqual(entryDto.Name, editedEntry.Name);
            Assert.AreEqual(
                entryDto.Description,
                EncryptionUtils.DecryptWithAES(editedEntry.DescriptionContent, editedEntry.DescriptionKey, editedEntry.DescriptionIV)
                );
            Assert.AreEqual(user.Id, editedEntry.UserId);
            Assert.AreEqual(entryDto.CategoryKeyword, editedEntry.Category.Keyword);
            Assert.AreEqual(entryDto.TagNames.Count, editedEntry.EntryTags.Count);

            foreach (var tag in editedEntry.EntryTags.Select(et => et.Tag.Name))
            {
                Assert.IsTrue(entryDto.TagNames.Contains(tag));
            }
        }

        private async Task AssertUnsuccessfulActionAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        {
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());

            var editedEntry = await databaseContext.Entries
                .Where(e => e.Id.Equals(entry.Id))
                .Include(e => e.Category)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .FirstAsync();

            Assert.AreEqual(entry.Date, editedEntry.Date);
            Assert.AreEqual(entry.Value, editedEntry.Value);
            Assert.AreEqual(entry.Name, editedEntry.Name);
            Assert.AreEqual(entry.DescriptionContent, editedEntry.DescriptionContent);
            Assert.AreEqual(entry.DescriptionKey, editedEntry.DescriptionKey);
            Assert.AreEqual(entry.DescriptionIV, editedEntry.DescriptionIV);
            Assert.AreEqual(entry.UserId, editedEntry.UserId);
            Assert.AreEqual(entry.Category.Keyword, editedEntry.Category.Keyword);
            Assert.AreEqual(entry.EntryTags.Count, editedEntry.EntryTags.Count);

            var editedEntryTags = editedEntry.EntryTags.Select(et => et.Tag.Name);
            foreach (var tag in entry.EntryTags.Select(et => et.Tag.Name))
            {
                Assert.IsTrue(editedEntryTags.Contains(tag));
            }
        }
    }
}
