using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests
{
    public abstract class AbstractTestClass
    {
        private CustomWebApplicationFactory<Program> customWebApplicationFactory;
        private HttpClient httpClient;
        protected IServiceProvider serviceProvider;
        protected DatabaseContext databaseContext;

        [SetUp]
        protected virtual void Setup()
        {
            customWebApplicationFactory = new CustomWebApplicationFactory<Program>(PrepareMocks);
            httpClient = customWebApplicationFactory.CreateClient();
            CreateNewScope();
            PrepareTestData();
            CreateNewScope();
        }

        [TearDown]
        protected virtual void TearDown()
        {
            databaseContext.Database.EnsureDeleted();
        }

        protected virtual void PrepareMocks(IServiceCollection services) { }

        protected abstract void PrepareTestData();

        protected T GetService<T>() where T : class
        {
            return serviceProvider.GetRequiredService<T>();
        }

        protected async Task<HttpResponseMessage> SendHttpRequestAsync(HttpMethod httpMethod,
            string url,
            string? accessToken = null,
            object? body = null)
        {
            var request = new HttpRequestMessage(httpMethod, url);
            if (accessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            if (body != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            }

            CreateNewScope();
            var response = await httpClient.SendAsync(request);
            CreateNewScope();

            return response;
        }

        private void CreateNewScope()
        {
            serviceProvider = customWebApplicationFactory.Services.CreateScope().ServiceProvider;
            databaseContext = serviceProvider.GetRequiredService<DatabaseContext>();
        }
    }
}
