using Application.Dtos;
using Application.Interfaces;
using Application.Utilities;
using Application.Utilities.Pagination;
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
    public class GetAllPagedTest : AbstractTestClass
    {
        private static readonly string URL = "/api/entry";

        private string accessToken;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<IConfiguration>(), databaseContext);
            var user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            GetService<IJwtService>().GenerateTokens(user, out accessToken, out _);
        }

        [Test]
        public async Task GetAllPaged_WithNoParams()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(6, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(6, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 7, 2, 15, 4, 3, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(60.45M, responseContent.Data[0].Value);
            Assert.AreEqual("Product 1", responseContent.Data[0].Name);
            Assert.AreEqual("Previous one broken down, bought again", responseContent.Data[0].Description);
            Assert.AreEqual("otherCost", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 25, 6, 34, 54, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(2.5M, responseContent.Data[1].Value);
            Assert.AreEqual("bread rolls", responseContent.Data[1].Name);
            Assert.IsNull(responseContent.Data[1].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[1].Tags.Count);
            Assert.AreEqual("secondTag", responseContent.Data[1].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 12, 14, 5, 21, DateTimeKind.Utc), responseContent.Data[2].Date);
            Assert.AreEqual(100.0M, responseContent.Data[2].Value);
            Assert.AreEqual("found some money", responseContent.Data[2].Name);
            Assert.AreEqual("Yay!", responseContent.Data[2].Description);
            Assert.AreEqual("otherIncome", responseContent.Data[2].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[2].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[2].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 10, 12, 0, 5, DateTimeKind.Utc), responseContent.Data[3].Date);
            Assert.AreEqual(3200.0M, responseContent.Data[3].Value);
            Assert.AreEqual("Salary", responseContent.Data[3].Name);
            Assert.AreEqual("Salary for producing in 06/22", responseContent.Data[3].Description);
            Assert.AreEqual("incomeCategory1", responseContent.Data[3].CategoryKeyword);
            Assert.AreEqual(0, responseContent.Data[3].Tags.Count);
            Assert.AreEqual(new DateTime(2022, 6, 2, 20, 15, 24, DateTimeKind.Utc), responseContent.Data[4].Date);
            Assert.AreEqual(60.45M, responseContent.Data[4].Value);
            Assert.AreEqual("Product 1", responseContent.Data[4].Name);
            Assert.AreEqual("Product 1 has been bought", responseContent.Data[4].Description);
            Assert.AreEqual("otherCost", responseContent.Data[4].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[4].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[4].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 5, 12, 17, 21, 21, DateTimeKind.Utc), responseContent.Data[5].Date);
            Assert.AreEqual(17.65M, responseContent.Data[5].Value);
            Assert.AreEqual("Bills", responseContent.Data[5].Name);
            Assert.AreEqual("Monthly bills", responseContent.Data[5].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[5].CategoryKeyword);
            Assert.AreEqual(2, responseContent.Data[5].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[5].Tags[0].Name);
            Assert.AreEqual("Tag number 3", responseContent.Data[5].Tags[1].Name);
        }

        [Test]
        public async Task GetAllPaged_WithSmallSecondPage()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?pageNumber=2&pageSize=2", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(2, responseContent.PageNumber);
            Assert.AreEqual(2, responseContent.PageSize);
            Assert.AreEqual(6, responseContent.TotalCount);
            Assert.IsFalse(responseContent.IsLastPage);

            Assert.AreEqual(2, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 6, 12, 14, 5, 21, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(100.0M, responseContent.Data[0].Value);
            Assert.AreEqual("found some money", responseContent.Data[0].Name);
            Assert.AreEqual("Yay!", responseContent.Data[0].Description);
            Assert.AreEqual("otherIncome", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 10, 12, 0, 5, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(3200.0M, responseContent.Data[1].Value);
            Assert.AreEqual("Salary", responseContent.Data[1].Name);
            Assert.AreEqual("Salary for producing in 06/22", responseContent.Data[1].Description);
            Assert.AreEqual("incomeCategory1", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(0, responseContent.Data[1].Tags.Count);
        }

        [Test]
        public async Task GetAllPaged_WithSortByDateAsc()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?pageSize=2&sortBy=date", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(2, responseContent.PageSize);
            Assert.AreEqual(6, responseContent.TotalCount);
            Assert.IsFalse(responseContent.IsLastPage);

            Assert.AreEqual(2, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 5, 12, 17, 21, 21, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(17.65M, responseContent.Data[0].Value);
            Assert.AreEqual("Bills", responseContent.Data[0].Name);
            Assert.AreEqual("Monthly bills", responseContent.Data[0].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(2, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual("Tag number 3", responseContent.Data[0].Tags[1].Name);
            Assert.AreEqual(new DateTime(2022, 6, 2, 20, 15, 24, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(60.45M, responseContent.Data[1].Value);
            Assert.AreEqual("Product 1", responseContent.Data[1].Name);
            Assert.AreEqual("Product 1 has been bought", responseContent.Data[1].Description);
            Assert.AreEqual("otherCost", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[1].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[1].Tags[0].Name);
        }

        [Test]
        public async Task GetAllPaged_WithSortByValueDesc()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?pageSize=2&sortBy=-value", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(2, responseContent.PageSize);
            Assert.AreEqual(6, responseContent.TotalCount);
            Assert.IsFalse(responseContent.IsLastPage);

            Assert.AreEqual(2, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 6, 10, 12, 0, 5, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(3200.0M, responseContent.Data[0].Value);
            Assert.AreEqual("Salary", responseContent.Data[0].Name);
            Assert.AreEqual("Salary for producing in 06/22", responseContent.Data[0].Description);
            Assert.AreEqual("incomeCategory1", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(0, responseContent.Data[0].Tags.Count);
            Assert.AreEqual(new DateTime(2022, 6, 12, 14, 5, 21, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(100.0M, responseContent.Data[1].Value);
            Assert.AreEqual("found some money", responseContent.Data[1].Name);
            Assert.AreEqual("Yay!", responseContent.Data[1].Description);
            Assert.AreEqual("otherIncome", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[1].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[1].Tags[0].Name);
        }

        [Test]
        public async Task GetAllPaged_WithSortByNameAsc()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?pageSize=2&sortBy=name", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(2, responseContent.PageSize);
            Assert.AreEqual(6, responseContent.TotalCount);
            Assert.IsFalse(responseContent.IsLastPage);

            Assert.AreEqual(2, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 5, 12, 17, 21, 21, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(17.65M, responseContent.Data[0].Value);
            Assert.AreEqual("Bills", responseContent.Data[0].Name);
            Assert.AreEqual("Monthly bills", responseContent.Data[0].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(2, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual("Tag number 3", responseContent.Data[0].Tags[1].Name);
            Assert.AreEqual(new DateTime(2022, 6, 25, 6, 34, 54, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(2.5M, responseContent.Data[1].Value);
            Assert.AreEqual("bread rolls", responseContent.Data[1].Name);
            Assert.IsNull(responseContent.Data[1].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[1].Tags.Count);
            Assert.AreEqual("secondTag", responseContent.Data[1].Tags[0].Name);
        }

        [Test]
        public async Task GetAllPaged_WithSearchValue()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?searchValue=pro", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(3, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 7, 2, 15, 4, 3, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(60.45M, responseContent.Data[0].Value);
            Assert.AreEqual("Product 1", responseContent.Data[0].Name);
            Assert.AreEqual("Previous one broken down, bought again", responseContent.Data[0].Description);
            Assert.AreEqual("otherCost", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 10, 12, 0, 5, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(3200.0M, responseContent.Data[1].Value);
            Assert.AreEqual("Salary", responseContent.Data[1].Name);
            Assert.AreEqual("Salary for producing in 06/22", responseContent.Data[1].Description);
            Assert.AreEqual("incomeCategory1", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(0, responseContent.Data[1].Tags.Count);
            Assert.AreEqual(new DateTime(2022, 6, 2, 20, 15, 24, DateTimeKind.Utc), responseContent.Data[2].Date);
            Assert.AreEqual(60.45M, responseContent.Data[2].Value);
            Assert.AreEqual("Product 1", responseContent.Data[2].Name);
            Assert.AreEqual("Product 1 has been bought", responseContent.Data[2].Description);
            Assert.AreEqual("otherCost", responseContent.Data[2].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[2].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[2].Tags[0].Name);
        }

        [Test]
        public async Task GetAllPaged_WithSearchValueWithNoResults()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?searchValue=nonExistantString", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(0, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(0, responseContent.Data.Count);
        }

        [Test]
        public async Task GetAllPaged_WithDateRange()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?dateFrom=2022-06-02&dateTo=2022-06-12", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(3, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 6, 12, 14, 5, 21, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(100.0M, responseContent.Data[0].Value);
            Assert.AreEqual("found some money", responseContent.Data[0].Name);
            Assert.AreEqual("Yay!", responseContent.Data[0].Description);
            Assert.AreEqual("otherIncome", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 10, 12, 0, 5, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(3200.0M, responseContent.Data[1].Value);
            Assert.AreEqual("Salary", responseContent.Data[1].Name);
            Assert.AreEqual("Salary for producing in 06/22", responseContent.Data[1].Description);
            Assert.AreEqual("incomeCategory1", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(0, responseContent.Data[1].Tags.Count);
            Assert.AreEqual(new DateTime(2022, 6, 2, 20, 15, 24, DateTimeKind.Utc), responseContent.Data[2].Date);
            Assert.AreEqual(60.45M, responseContent.Data[2].Value);
            Assert.AreEqual("Product 1", responseContent.Data[2].Name);
            Assert.AreEqual("Product 1 has been bought", responseContent.Data[2].Description);
            Assert.AreEqual("otherCost", responseContent.Data[2].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[2].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[2].Tags[0].Name);
        }

        [Test]
        public async Task GetAllPaged_WithCategoriesKeywords()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?categoriesKeywords=costcategory2,otherincome", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(3, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 6, 25, 6, 34, 54, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(2.5M, responseContent.Data[0].Value);
            Assert.AreEqual("bread rolls", responseContent.Data[0].Name);
            Assert.IsNull(responseContent.Data[0].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("secondTag", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 12, 14, 5, 21, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(100.0M, responseContent.Data[1].Value);
            Assert.AreEqual("found some money", responseContent.Data[1].Name);
            Assert.AreEqual("Yay!", responseContent.Data[1].Description);
            Assert.AreEqual("otherIncome", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[1].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[1].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 5, 12, 17, 21, 21, DateTimeKind.Utc), responseContent.Data[2].Date);
            Assert.AreEqual(17.65M, responseContent.Data[2].Value);
            Assert.AreEqual("Bills", responseContent.Data[2].Name);
            Assert.AreEqual("Monthly bills", responseContent.Data[2].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[2].CategoryKeyword);
            Assert.AreEqual(2, responseContent.Data[2].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[2].Tags[0].Name);
            Assert.AreEqual("Tag number 3", responseContent.Data[2].Tags[1].Name);
        }

        [Test]
        public async Task GetAllPaged_WithTagsNames()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?tagsNames=Tag number 3,tag of another user", accessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(3, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(3, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 7, 2, 15, 4, 3, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(60.45M, responseContent.Data[0].Value);
            Assert.AreEqual("Product 1", responseContent.Data[0].Name);
            Assert.AreEqual("Previous one broken down, bought again", responseContent.Data[0].Description);
            Assert.AreEqual("otherCost", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[0].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 6, 2, 20, 15, 24, DateTimeKind.Utc), responseContent.Data[1].Date);
            Assert.AreEqual(60.45M, responseContent.Data[1].Value);
            Assert.AreEqual("Product 1", responseContent.Data[1].Name);
            Assert.AreEqual("Product 1 has been bought", responseContent.Data[1].Description);
            Assert.AreEqual("otherCost", responseContent.Data[1].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[1].Tags.Count);
            Assert.AreEqual("Tag number 3", responseContent.Data[1].Tags[0].Name);
            Assert.AreEqual(new DateTime(2022, 5, 12, 17, 21, 21, DateTimeKind.Utc), responseContent.Data[2].Date);
            Assert.AreEqual(17.65M, responseContent.Data[2].Value);
            Assert.AreEqual("Bills", responseContent.Data[2].Name);
            Assert.AreEqual("Monthly bills", responseContent.Data[2].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[2].CategoryKeyword);
            Assert.AreEqual(2, responseContent.Data[2].Tags.Count);
            Assert.AreEqual("tag1", responseContent.Data[2].Tags[0].Name);
            Assert.AreEqual("Tag number 3", responseContent.Data[2].Tags[1].Name);
        }

        [Test]
        public async Task GetAllPaged_ForUserWithUnverifiedEmail()
        {
            string anotherAccessToken;
            var anotherUser = databaseContext.Users.Where(u => !u.IsEmailVerified).First();
            GetService<IJwtService>().GenerateTokens(anotherUser, out anotherAccessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, anotherAccessToken);
            var responseContent = await GetResponseContentAsync<Page<EntryDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.PageNumber);
            Assert.AreEqual(10, responseContent.PageSize);
            Assert.AreEqual(1, responseContent.TotalCount);
            Assert.IsTrue(responseContent.IsLastPage);

            Assert.AreEqual(1, responseContent.Data.Count);
            Assert.AreEqual(new DateTime(2022, 6, 16, 17, 7, 43, DateTimeKind.Utc), responseContent.Data[0].Date);
            Assert.AreEqual(15.15M, responseContent.Data[0].Value);
            Assert.AreEqual("Food", responseContent.Data[0].Name);
            Assert.IsNull(responseContent.Data[0].Description);
            Assert.AreEqual("costCategory2", responseContent.Data[0].CategoryKeyword);
            Assert.AreEqual(1, responseContent.Data[0].Tags.Count);
            Assert.AreEqual("tag of another user", responseContent.Data[0].Tags[0].Name);
        }

        [Test]
        public async Task GetAllPaged_WithWrongSortBy()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?sortBy=someTotallyWrongValue", accessToken);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }

        [Test]
        public async Task GetAllPaged_WithWrongDateRange()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, $"{URL}?dateFrom=2022-06-15&dateTo=2022-06-01", accessToken);
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }

        [Test]
        public async Task GetAllPaged_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, "someTotallyWrongAccessToken");
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
