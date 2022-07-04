using Application.Dtos.Outgoing;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests.CategoryController
{
    public class GetAllTest : AbstractTestClass
    {
        private static readonly string URL = "/api/category";

        protected override void PrepareTestData()
        {
            DataSeeder.SeedUsers(GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task GetAll_Authorized()
        {
            string accessToken;
            GetService<IJwtService>().GenerateTokens(databaseContext.Users.First(), out accessToken, out _);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, accessToken);
            var responseContent = await GetResponseContentAsync<ApiResponse<List<CategoryDto>>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);
            Assert.AreEqual(6, responseContent.Data.Count);
            Assert.NotNull(responseContent.Data[0].Id);
            Assert.AreEqual("category1", responseContent.Data[0].Keyword);
            Assert.AreEqual(true, responseContent.Data[0].IsIncome);
            Assert.AreEqual("icon data 1", responseContent.Data[0].Icon);
            Assert.AreEqual("#654321", responseContent.Data[0].IconColor);
            Assert.NotNull(responseContent.Data[1].Id);
            Assert.AreEqual("category2", responseContent.Data[1].Keyword);
            Assert.AreEqual(true, responseContent.Data[1].IsIncome);
            Assert.AreEqual("icon data 2", responseContent.Data[1].Icon);
            Assert.AreEqual("#654321", responseContent.Data[1].IconColor);
            Assert.NotNull(responseContent.Data[2].Id);
            Assert.AreEqual("otherIncome", responseContent.Data[2].Keyword);
            Assert.AreEqual(true, responseContent.Data[2].IsIncome);
            Assert.AreEqual("icon data 3", responseContent.Data[2].Icon);
            Assert.AreEqual("#654321", responseContent.Data[2].IconColor);
            Assert.NotNull(responseContent.Data[3].Id);
            Assert.AreEqual("category4", responseContent.Data[3].Keyword);
            Assert.AreEqual(false, responseContent.Data[3].IsIncome);
            Assert.AreEqual("icon data 4", responseContent.Data[3].Icon);
            Assert.AreEqual("#123456", responseContent.Data[3].IconColor);
            Assert.NotNull(responseContent.Data[4].Id);
            Assert.AreEqual("category5", responseContent.Data[4].Keyword);
            Assert.AreEqual(false, responseContent.Data[4].IsIncome);
            Assert.AreEqual("icon data 5", responseContent.Data[4].Icon);
            Assert.AreEqual("#123456", responseContent.Data[4].IconColor);
            Assert.NotNull(responseContent.Data[5].Id);
            Assert.AreEqual("otherCost", responseContent.Data[5].Keyword);
            Assert.AreEqual(false, responseContent.Data[5].IsIncome);
            Assert.AreEqual("icon data 6", responseContent.Data[5].Icon);
            Assert.AreEqual("#123456", responseContent.Data[5].IconColor);
        }

        [Test]
        public async Task GetAll_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, "someTotallyWrongAccessToken");
            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
