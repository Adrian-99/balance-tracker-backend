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

namespace APITest.Tests.EntryController
{
    public class DeleteTest : AbstractTestClass
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
            entry = databaseContext.Entries.Include(e => e.EntryTags)
                .Where(e => e.EntryTags.Count > 1 && e.UserId.Equals(user.Id))
                .First();

            entriesCountBefore = databaseContext.Entries.Count();
            entryTagsCountBefore = databaseContext.EntryTags.Count();
        }

        [Test]
        public async Task Delete_EntryWithTags()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL + entry.Id, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore - 1, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore - entry.EntryTags.Count, databaseContext.EntryTags.Count());
            Assert.AreEqual(0, databaseContext.Entries.Where(e => e.Id.Equals(entry.Id)).Count());
            Assert.AreEqual(0, databaseContext.EntryTags.Where(et => et.EntryId.Equals(entry.Id)).Count());
        }

        [Test]
        public async Task Delete_EntryWithoutTags()
        {
            entry = await databaseContext.Entries.Include(e => e.EntryTags)
                .Where(e => e.EntryTags.Count.Equals(0) && e.UserId.Equals(user.Id))
                .FirstAsync();

            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL + entry.Id, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore - 1, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
            Assert.AreEqual(0, databaseContext.Entries.Where(e => e.Id.Equals(entry.Id)).Count());
        }

        [Test]
        public async Task Delete_EntryOfAnotherUser()
        {
            entry = await databaseContext.Entries
                .Where(e => !e.UserId.Equals(user.Id))
                .FirstAsync();

            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL + entry.Id, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
            Assert.AreEqual(1, databaseContext.Entries.Where(e => e.Id.Equals(entry.Id)).Count());
        }

        [Test]
        public async Task Delete_ForUserWithUnverifiedEmail()
        {
            user = await databaseContext.Users.Where(u => !u.IsEmailVerified).FirstAsync();
            tokens = GetService<IJwtService>().GenerateTokens(user);
            entry = await databaseContext.Entries
                .Where(e => e.UserId.Equals(user.Id))
                .FirstAsync();

            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL + entry.Id, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
            Assert.AreEqual(1, databaseContext.Entries.Where(e => e.Id.Equals(entry.Id)).Count());
        }

        [Test]
        public async Task Delete_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Delete, URL + entry.Id, "someTotallyWrongAccessToken");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);

            Assert.AreEqual(entriesCountBefore, databaseContext.Entries.Count());
            Assert.AreEqual(entryTagsCountBefore, databaseContext.EntryTags.Count());
            Assert.AreEqual(1, databaseContext.Entries.Where(e => e.Id.Equals(entry.Id)).Count());
        }
    }
}
