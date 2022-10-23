using Application;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Application.Utilities;
using Application.Utilities.Pagination;
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
    public class GetAllPagedTest : AbstractTestClass
    {
        private static readonly string URL = "api/tag";

        private User user;
        private JwtTokens tokens;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<CategoriesLoader>(), GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);
        }

        [Test]
        public async Task GetAllPaged_WithNoParams()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<Page<TagDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(3, responseContent.Data.Count);
            Assert.AreEqual("secondTag", responseContent.Data[0].Name);
            Assert.AreEqual(1, responseContent.Data[0].EntriesCount);
            Assert.AreEqual("Tag number 3", responseContent.Data[1].Name);
            Assert.AreEqual(3, responseContent.Data[1].EntriesCount);
            Assert.AreEqual("tag1", responseContent.Data[2].Name);
            Assert.AreEqual(2, responseContent.Data[2].EntriesCount);
        }

        [Test]
        public async Task GetAllPaged_WithSmallSecondPage()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?pageSize=1&pageNumber=2", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<Page<TagDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(2, responseContent.PageNumber);
            Assert.AreEqual(1, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsFalse(responseContent.IsLastPage);

            Assert.AreEqual(1, responseContent.Data.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[0].Name);
            Assert.AreEqual(3, responseContent.Data[0].EntriesCount);
        }

        [Test]
        public async Task GetAllPaged_WithSortByNameDesc()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?sortBy=-name", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<Page<TagDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(3, responseContent.Data.Count);
            Assert.AreEqual("tag1", responseContent.Data[0].Name);
            Assert.AreEqual(2, responseContent.Data[0].EntriesCount);
            Assert.AreEqual("Tag number 3", responseContent.Data[1].Name);
            Assert.AreEqual(3, responseContent.Data[1].EntriesCount);
            Assert.AreEqual("secondTag", responseContent.Data[2].Name);
            Assert.AreEqual(1, responseContent.Data[2].EntriesCount);
        }

        [Test]
        public async Task GetAllPaged_WithSortByEntriesCountAsc()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?sortBy=entriesCount", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<Page<TagDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(3, responseContent.Data.Count);
            Assert.AreEqual("secondTag", responseContent.Data[0].Name);
            Assert.AreEqual(1, responseContent.Data[0].EntriesCount);
            Assert.AreEqual("tag1", responseContent.Data[1].Name);
            Assert.AreEqual(2, responseContent.Data[1].EntriesCount);
            Assert.AreEqual("Tag number 3", responseContent.Data[2].Name);
            Assert.AreEqual(3, responseContent.Data[2].EntriesCount);
        }

        [Test]
        public async Task GetAllPaged_WithSeachValue()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?searchValue=N", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<Page<TagDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(2, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(2, responseContent.Data.Count);
            Assert.AreEqual("secondTag", responseContent.Data[0].Name);
            Assert.AreEqual(1, responseContent.Data[0].EntriesCount);
            Assert.AreEqual("Tag number 3", responseContent.Data[1].Name);
            Assert.AreEqual(3, responseContent.Data[1].EntriesCount);
        }

        [Test]
        public async Task GetAllPaged_WithSeachValueWithNoResults()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?searchValue=nonExistantValue", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<Page<TagDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(0, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(0, responseContent.Data.Count);
        }

        [Test]
        public async Task GetAllPaged_ForUserWithUnverifiedEmail()
        {
            user = databaseContext.Users.Where(u => !u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<Page<TagDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(1, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(1, responseContent.Data.Count);
            Assert.AreEqual("tag of another user", responseContent.Data[0].Name);
            Assert.AreEqual(1, responseContent.Data[0].EntriesCount);
        }

        [Test]
        public async Task GetAllPaged_WithWrongSortBy()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?sortBy=someInvalidName", tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }

        [Test]
        public async Task GetAllPaged_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, "someTotallyWrongAccessToken");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
