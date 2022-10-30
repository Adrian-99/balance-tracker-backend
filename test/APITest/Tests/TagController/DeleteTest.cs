using Application;
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
    public class DeleteTest : AbstractTestClass
    {
        private static readonly string URL = "api/tag/";

        private User user;
        private JwtTokens tokens;
        private Tag tag;
        private int tagsCountBefore;
        private int entryTagsCountBefore;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<CategoriesLoader>(), GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);
            tag = databaseContext.Tags.Where(t => t.UserId.Equals(user.Id) && t.Name.Equals("Tag number 3")).First();

            tagsCountBefore = databaseContext.Tags.Count();
            entryTagsCountBefore = databaseContext.EntryTags.Count();
        }

        [Test]
        public async Task Delete_WithNoReplacement()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Delete, $"{URL}{tag.Id}", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore - 1, databaseContext.Tags.Count());
            Assert.AreEqual(entryTagsCountBefore - 3, databaseContext.EntryTags.Count());
            Assert.AreEqual(0, databaseContext.Tags.Where(t => t.Id.Equals(tag.Id)).Count());
            Assert.AreEqual(0, databaseContext.EntryTags.Where(et => et.TagId.Equals(tag.Id)).Count());
        }

        [Test]
        public async Task Delete_WithReplacement()
        {
            var affectedEntries = await databaseContext.EntryTags
                .Include(et => et.Entry)
                .Where(et => et.TagId.Equals(tag.Id))
                .Select(et => et.Entry.Id)
                .ToListAsync();

            var response = await SendHttpRequestAsync(HttpMethod.Delete, $"{URL}{tag.Id}?replacementTags=tag1,4th tag", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore - 1, databaseContext.Tags.Count());
            Assert.AreEqual(entryTagsCountBefore + 2, databaseContext.EntryTags.Count());
            Assert.AreEqual(0, databaseContext.Tags.Where(t => t.Id.Equals(tag.Id)).Count());
            Assert.AreEqual(0, databaseContext.EntryTags.Where(et => et.TagId.Equals(tag.Id)).Count());

            var affectedEntriesEntryTags = await databaseContext.Entries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => affectedEntries.Contains(e.Id))
                .Select(e => e.EntryTags)
                .ToListAsync();
            foreach (var affectedEntryEntryTags in affectedEntriesEntryTags)
            {
                Assert.IsTrue(affectedEntryEntryTags.Any(et => et.Tag.Name.Equals("tag1")));
                Assert.IsTrue(affectedEntryEntryTags.Any(et => et.Tag.Name.Equals("4th tag")));
            }
        }

        [Test]
        public async Task Delete_TagWithNoEntriesAssigend()
        {
            tag = await databaseContext.Tags
                .Include(t => t.EntryTags)
                .Where(t => t.UserId.Equals(user.Id) && t.EntryTags.Count.Equals(0))
                .FirstAsync();

            var response = await SendHttpRequestAsync(HttpMethod.Delete, $"{URL}{tag.Id}?replacementTags=tag1", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore - 1, databaseContext.Tags.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
            Assert.AreEqual(0, databaseContext.Tags.Where(t => t.Id.Equals(tag.Id)).Count());
            Assert.AreEqual(0, databaseContext.EntryTags.Where(et => et.TagId.Equals(tag.Id)).Count());
        }

        [Test]
        public async Task Delete_TagOfAnotherUser()
        {
            tag = await databaseContext.Tags.Where(t => !t.UserId.Equals(user.Id)).FirstAsync();

            var response = await SendHttpRequestAsync(HttpMethod.Delete, $"{URL}{tag.Id}", tokens.AccessToken);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.NotFound);
        }

        [Test]
        public async Task Delete_WithIncorrectReplacement()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Delete, $"{URL}{tag.Id}?replacementTags=tag of another user", tokens.AccessToken);
            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Delete_ForUserWithUnverifiedEmail()
        {
            user = await databaseContext.Users.Where(u => !u.IsEmailVerified).FirstAsync();
            tokens = GetService<IJwtService>().GenerateTokens(user);
            tag = await databaseContext.Tags.Where(t => t.UserId.Equals(user.Id)).FirstAsync();

            var response = await SendHttpRequestAsync(HttpMethod.Delete, $"{URL}{tag.Id}", tokens.AccessToken);

            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task Delete_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Delete, $"{URL}{tag.Id}", "someTotallyWrongAccessToken");
            await AssertUnsuccessfulActionAsync(response, HttpStatusCode.Unauthorized);
        }

        private async Task AssertUnsuccessfulActionAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        {
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(tagsCountBefore, databaseContext.Tags.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
            Assert.AreEqual(1, databaseContext.Tags.Where(t => t.Id.Equals(tag.Id)).Count());
        }
    }
}
