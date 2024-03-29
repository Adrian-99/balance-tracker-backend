﻿using Application;
using Application.Dtos.Ingoing;
using Application.Interfaces;
using Application.Utilities;
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
    public class GetAllNamesTest : AbstractTestClass
    {
        private static readonly string URL = "/api/tag/name";

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<CategoriesLoader>(), GetService<IConfiguration>(), databaseContext);
        }

        [Test]
        public async Task GetAllUnpaged()
        {
            var user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            var tokens = GetService<IJwtService>().GenerateTokens(user);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<List<string>>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(4, responseContent.Data.Count);
            Assert.AreEqual("4th tag", responseContent.Data[0]);
            Assert.AreEqual("secondTag", responseContent.Data[1]);
            Assert.AreEqual("Tag number 3", responseContent.Data[2]);
            Assert.AreEqual("tag1", responseContent.Data[3]);
        }

        [Test]
        public async Task GetAllUnpaged_ForUserWithUnverifiedEmail()
        {
            var user = databaseContext.Users.Where(u => !u.IsEmailVerified).First();
            var tokens = GetService<IJwtService>().GenerateTokens(user);

            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, tokens.AccessToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<List<string>>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(1, responseContent.Data.Count);
            Assert.AreEqual("tag of another user", responseContent.Data[0]);
        }

        [Test]
        public async Task GetAllUnpaged_Unauthorized()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL, "someTotallyWrongAccessToken");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
