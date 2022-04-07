using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest
{
    public abstract class AbstractControllerTest
    {
        private CustomWebApplicationFactory<Program> customWebApplicationFactory;
        protected HttpClient httpClient;
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

        private void CreateNewScope()
        {
            serviceProvider = customWebApplicationFactory.Services.CreateScope().ServiceProvider;
            databaseContext = serviceProvider.GetRequiredService<DatabaseContext>();
        }
    }
}
