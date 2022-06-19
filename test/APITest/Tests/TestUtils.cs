using Domain.Entities;
using Infrastructure.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests
{
    public static class TestUtils
    {
        public static User GetUserById(DatabaseContext databaseContext, Guid userId)
        {
            return (from user in databaseContext.Users
                    where user.Id == userId
                    select user).First();
        }
    }
}
