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
        public static Task<HttpResponseMessage> SendHttpRequestAsync(HttpClient httpClient,
            HttpMethod httpMethod,
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

            return httpClient.SendAsync(request);
        }
    }
}
