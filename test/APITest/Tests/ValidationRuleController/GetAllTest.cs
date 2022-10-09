using Application.Dtos.Outgoing;
using Application.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests.ValidationRuleController
{
    public class GetAllTest : AbstractTestClass
    {
        private static readonly string URL = "/api/validation-rule/all";

        protected override void PrepareTestData() { }

        [Test]
        public async Task GetAll()
        {
            var response = await SendHttpRequestAsync(HttpMethod.Get, URL);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<ValidationRulesDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(40, responseContent.Data.UserUsernameMaxLength);
            Assert.AreEqual(7, responseContent.Data.UserUsernameAllowedChangeFrequencyDays);
            Assert.AreEqual(30, responseContent.Data.UserFirstNameMaxLength);
            Assert.AreEqual(35, responseContent.Data.UserLastNameMaxLength);
            Assert.AreEqual(8, responseContent.Data.UserPasswordMinLength);
            Assert.AreEqual(45, responseContent.Data.UserPasswordMaxLength);
            Assert.IsTrue(responseContent.Data.UserPasswordSmallLetterRequired);
            Assert.IsTrue(responseContent.Data.UserPasswordBigLetterRequired);
            Assert.IsTrue(responseContent.Data.UserPasswordDigitRequired);
            Assert.IsTrue(responseContent.Data.UserPasswordSpecialCharacterRequired);
            Assert.IsTrue(responseContent.Data.UserPasswordForbidSameAsUsername);
            Assert.IsTrue(responseContent.Data.UserPasswordForbidSameAsCurrent);
            Assert.AreEqual(25, responseContent.Data.EntryNameMaxLength);
            Assert.AreEqual(100, responseContent.Data.EntryDescriptionMaxLength);
            Assert.AreEqual(20, responseContent.Data.TagNameMaxLength);
        }
    }
}
