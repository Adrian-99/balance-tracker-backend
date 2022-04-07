using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public static HttpRequestMessage AuthorizedHttpRequest(HttpMethod httpMethod, string url, string accessToken)
        {
            var request = new HttpRequestMessage(httpMethod, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return request;
        }
    }
}
