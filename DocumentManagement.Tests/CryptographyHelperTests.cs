using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class CryptographyHelperTests
    {
        [TestMethod]
        public void Can_create_hash_and_salt()
        {
            CryptographyHelper.CreatePasswordHash("abracadabra", out byte[] hash, out byte[] salt);
            Assert.IsNotNull(hash);
            Assert.IsNotNull(salt);
            Assert.IsTrue(hash.Length > 0);
            Assert.IsTrue(salt.Length > 0);
        }

        [TestMethod]
        public void Can_create_different_hash_and_salt_for_several_calls()
        {
            CryptographyHelper.CreatePasswordHash("abracadabra", out byte[] hash1, out byte[] salt1);
            CryptographyHelper.CreatePasswordHash("abracadabra", out byte[] hash2, out byte[] salt2);

            CollectionAssert.AreNotEqual(hash1, hash2);
            CollectionAssert.AreNotEqual(salt1, salt2);
        }

        [TestMethod]
        public void Can_verify_password()
        {
            CryptographyHelper.CreatePasswordHash("abracadabra", out byte[] hash1, out byte[] salt1);
            Assert.IsTrue(CryptographyHelper.VerifyPasswordHash("abracadabra", hash1, salt1));
            Assert.IsFalse(CryptographyHelper.VerifyPasswordHash("cadabra", hash1, salt1));
        }

        [TestMethod]
        public void Can_verify_invalid_input()
        {
            CryptographyHelper.CreatePasswordHash("abracadabra", out byte[] hash1, out byte[] salt1);
            Assert.IsFalse(CryptographyHelper.VerifyPasswordHash(null, hash1, salt1));
            Assert.IsFalse(CryptographyHelper.VerifyPasswordHash(string.Empty, hash1, salt1));
            Assert.IsFalse(CryptographyHelper.VerifyPasswordHash("abracadabra", null, null));
            Assert.IsFalse(CryptographyHelper.VerifyPasswordHash("abracadabra", new byte[] { 1 }, new byte[] { 2, 3 }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Can_not_use_empty_password_1()
        {
            CryptographyHelper.CreatePasswordHash(null, out byte[] hash1, out byte[] salt1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Can_not_use_empty_password_2()
        {
            CryptographyHelper.CreatePasswordHash(string.Empty, out byte[] hash1, out byte[] salt1);
        }
    }
}
