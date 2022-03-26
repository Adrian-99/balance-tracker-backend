using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest
{
    public abstract class AbstractControllerTest
    {
        protected HttpClient httpClient;
        protected IServiceProvider serviceProvider;
        protected DatabaseContext databaseContext;

        public AbstractControllerTest()
        {
            var customWebApplicationFactory = new CustomWebApplicationFactory<Program>();
            httpClient = customWebApplicationFactory.CreateClient();
            serviceProvider = customWebApplicationFactory.Services.CreateScope().ServiceProvider;
            databaseContext = serviceProvider.GetRequiredService<DatabaseContext>();
        }

        protected virtual void ClearDatabaseContext()
        {
            databaseContext.Database.EnsureDeleted();
        }
    }
}
