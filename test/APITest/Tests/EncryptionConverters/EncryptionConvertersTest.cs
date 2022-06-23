using Application.Settings;
using Infrastructure.Data.Encryption;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests.EncryptionConverters
{
    public class EncryptionConvertersTest
    {
        private EncryptionSettings encryptionSettings1;
        private EncryptionSettings encryptionSettings2;

        [SetUp]
        public void Setup()
        {
            encryptionSettings1 = new EncryptionSettings();
            encryptionSettings1.PrivateKeyPath = "TestResources/Keys/PrivateKey.pem";
            encryptionSettings1.PrivateKeyPassword = "AUCZJ.p!J_fyXA4WZKX4sQ2VJ8#!7vLYCKx2Sm;,";
            encryptionSettings1.PublicKeyPath = "TestResources/Keys/PublicKey.pem";

            encryptionSettings2 = new EncryptionSettings();
            encryptionSettings2.PrivateKeyPath = "TestResources/Keys/PrivateKey2.pem";
            encryptionSettings2.PrivateKeyPassword = "";
            encryptionSettings2.PublicKeyPath = "TestResources/Keys/PublicKey2.pem";
        }

        [Test]
        public void DateTimeEncryptionConverterTest()
        {
            AssertDateTimeEncryptionConverter(new DateTimeEncryptionConverter(encryptionSettings1), DateTime.UtcNow);
            AssertDateTimeEncryptionConverter(new DateTimeEncryptionConverter(encryptionSettings1), DateTime.Now);

            AssertDateTimeEncryptionConverter(new DateTimeEncryptionConverter(encryptionSettings2), DateTime.UtcNow);
            AssertDateTimeEncryptionConverter(new DateTimeEncryptionConverter(encryptionSettings2), DateTime.Now);
        }

        [Test]
        public void DecimalEncryptionConverterTest()
        {
            AssertValueConverter(new DecimalEncryptionConverter(encryptionSettings1), 15.67M);
            AssertValueConverter(new DecimalEncryptionConverter(encryptionSettings1), 7M);

            AssertValueConverter(new DecimalEncryptionConverter(encryptionSettings2), 15.67M);
            AssertValueConverter(new DecimalEncryptionConverter(encryptionSettings2), 7M);
        }

        [Test]
        public void OptionalStringEncryptionConverterTest()
        {
            AssertValueConverter(new OptionalStringEncryptionConverter(encryptionSettings1), "Some text to encrypt");
            AssertValueConverter(new OptionalStringEncryptionConverter(encryptionSettings1), "");
            AssertValueConverter(new OptionalStringEncryptionConverter(encryptionSettings1), null);
            
            AssertValueConverter(new OptionalStringEncryptionConverter(encryptionSettings2), "Some text to encrypt");
            AssertValueConverter(new OptionalStringEncryptionConverter(encryptionSettings2), "");
            AssertValueConverter(new OptionalStringEncryptionConverter(encryptionSettings2), null);
        }

        [Test]
        public void StringEncryptionConverterTest()
        {
            AssertValueConverter(new StringEncryptionConverter(encryptionSettings1), "Some text to encrypt");
            AssertValueConverter(new StringEncryptionConverter(encryptionSettings1), "");

            AssertValueConverter(new StringEncryptionConverter(encryptionSettings2), "Some text to encrypt");
            AssertValueConverter(new StringEncryptionConverter(encryptionSettings2), "");
        }

        private void AssertDateTimeEncryptionConverter(DateTimeEncryptionConverter converter, DateTime initialValue)
        {
            var encryptedValue = converter.ConvertToProvider.Invoke(initialValue);
            var decryptedValue = converter.ConvertFromProvider.Invoke(encryptedValue);

            Assert.AreNotEqual(initialValue, encryptedValue);
            Assert.AreEqual(initialValue.ToUniversalTime(), decryptedValue);
        }

        private void AssertValueConverter<T>(ValueConverter<T, string> converter, T initialValue)
        {
            var encryptedValue = converter.ConvertToProvider.Invoke(initialValue);
            var decryptedValue = converter.ConvertFromProvider.Invoke(encryptedValue);

            if (initialValue != null)
            {
                Assert.AreNotEqual(initialValue, encryptedValue);
            }
            Assert.AreEqual(initialValue, decryptedValue);
        }
    }
}
