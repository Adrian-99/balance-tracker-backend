using Application.Dtos.Outgoing;
using Application.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests.UserController
{
    public class GetUserSettingsTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/settings";

        protected override void PrepareTestData() { }

        [Test]
        public async Task GetUserSettings()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL);
            var responseContent = await GetResponseContentAsync<ApiResponse<UserSettingsDto>>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(40, responseContent.Data.UsernameMaxLength);
            Assert.AreEqual(7, responseContent.Data.UsernameAllowedChangeFrequencyDays);
            Assert.AreEqual(30, responseContent.Data.FirstNameMaxLength);
            Assert.AreEqual(35, responseContent.Data.LastNameMaxLength);
            Assert.AreEqual(8, responseContent.Data.PasswordMinLength);
            Assert.AreEqual(45, responseContent.Data.PasswordMaxLength);
            Assert.IsTrue(responseContent.Data.PasswordSmallLetterRequired);
            Assert.IsTrue(responseContent.Data.PasswordBigLetterRequired);
            Assert.IsTrue(responseContent.Data.PasswordDigitRequired);
            Assert.IsTrue(responseContent.Data.PasswordSpecialCharacterRequired);
            Assert.IsTrue(responseContent.Data.PasswordForbidSameAsUsername);
            Assert.IsTrue(responseContent.Data.PasswordForbidSameAsCurrent);
        }
    }
}
