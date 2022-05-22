using Application.Dtos.Outgoing;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest.UserController
{
    public class GetUserSettingsTest : AbstractTestClass
    {
        private static readonly string URL = "/api/user/settings";

        protected override void PrepareTestData() { }

        [Test]
        public async Task GetUserSettings()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL);
            var responseContent = JsonConvert.DeserializeObject<UserSettingsDto>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            Assert.AreEqual(40, responseContent.UsernameMaxLength);
            Assert.AreEqual(7, responseContent.UsernameAllowedChangeFrequencyDays);
            Assert.AreEqual(30, responseContent.FirstNameMaxLength);
            Assert.AreEqual(35, responseContent.LastNameMaxLength);
            Assert.AreEqual(8, responseContent.PasswordMinLength);
            Assert.AreEqual(45, responseContent.PasswordMaxLength);
            Assert.AreEqual(true, responseContent.PasswordSmallLetterRequired);
            Assert.AreEqual(true, responseContent.PasswordBigLetterRequired);
            Assert.AreEqual(true, responseContent.PasswordDigitRequired);
            Assert.AreEqual(true, responseContent.PasswordSpecialCharacterRequired);
            Assert.AreEqual(true, responseContent.PasswordForbidSameAsUsername);
            Assert.AreEqual(true, responseContent.PasswordForbidSameAsCurrent);
        }
    }
}
