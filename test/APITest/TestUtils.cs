using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest
{
    public static class TestUtils
    {
        public static Task<HttpResponseMessage> PostWithJsonBodyAsync(HttpClient httpClient, string url, object body)
        {
            return httpClient.PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"
                ));
        }
    }
}
