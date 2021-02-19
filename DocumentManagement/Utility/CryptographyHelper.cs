using System;
using System.Linq;
using System.Security.Cryptography;

namespace MRS.DocumentManagement.Utility
{
    public class CryptographyHelper
    {
        public virtual bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(password) || passwordHash.Length == 0 || passwordSalt.Length == 0)
                return false;

            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return passwordHash.SequenceEqual(computedHash);
        }

        public virtual void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must not be empty", nameof(password));

            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
