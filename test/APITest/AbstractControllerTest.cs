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

        public AbstractControllerTest()
        {
            customWebApplicationFactory = new CustomWebApplicationFactory<Program>();
            httpClient = customWebApplicationFactory.CreateClient();
        }

        [SetUp]
        protected virtual void Setup()
        {
            CreateNewScope();
            PrepareDatabase();
            CreateNewScope();
        }

        [TearDown]
        protected virtual void TearDown()
        {
            databaseContext.Database.EnsureDeleted();
        }

        protected abstract void PrepareDatabase();

        private void CreateNewScope()
        {
            serviceProvider = customWebApplicationFactory.Services.CreateScope().ServiceProvider;
            databaseContext = serviceProvider.GetRequiredService<DatabaseContext>();
        }
    }
}
