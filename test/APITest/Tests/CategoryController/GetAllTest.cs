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
            var responseContent = JsonConvert.DeserializeObject<List<CategoryDto>>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.AreEqual(6, responseContent.Count);
            Assert.NotNull(responseContent[0].Id);
            Assert.AreEqual("category1", responseContent[0].Keyword);
            Assert.AreEqual(true, responseContent[0].IsIncome);
            Assert.AreEqual("icon data 1", responseContent[0].Icon);
            Assert.AreEqual("#654321", responseContent[0].IconColor);
            Assert.NotNull(responseContent[1].Id);
            Assert.AreEqual("category2", responseContent[1].Keyword);
            Assert.AreEqual(true, responseContent[1].IsIncome);
            Assert.AreEqual("icon data 2", responseContent[1].Icon);
            Assert.AreEqual("#654321", responseContent[1].IconColor);
            Assert.NotNull(responseContent[2].Id);
            Assert.AreEqual("otherIncome", responseContent[2].Keyword);
            Assert.AreEqual(true, responseContent[2].IsIncome);
            Assert.AreEqual("icon data 3", responseContent[2].Icon);
            Assert.AreEqual("#654321", responseContent[2].IconColor);
            Assert.NotNull(responseContent[3].Id);
            Assert.AreEqual("category4", responseContent[3].Keyword);
            Assert.AreEqual(false, responseContent[3].IsIncome);
            Assert.AreEqual("icon data 4", responseContent[3].Icon);
            Assert.AreEqual("#123456", responseContent[3].IconColor);
            Assert.NotNull(responseContent[4].Id);
            Assert.AreEqual("category5", responseContent[4].Keyword);
            Assert.AreEqual(false, responseContent[4].IsIncome);
            Assert.AreEqual("icon data 5", responseContent[4].Icon);
            Assert.AreEqual("#123456", responseContent[4].IconColor);
            Assert.NotNull(responseContent[5].Id);
            Assert.AreEqual("otherCost", responseContent[5].Keyword);
            Assert.AreEqual(false, responseContent[5].IsIncome);
            Assert.AreEqual("icon data 6", responseContent[5].Icon);
            Assert.AreEqual("#123456", responseContent[5].IconColor);
        }

        [Test]
        public async Task GetAll_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, "someTotallyWrongAccessToken");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
