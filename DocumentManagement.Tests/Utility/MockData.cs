using System.Collections.Generic;
using DocumentManagement.Database.Models;

namespace DocumentManagement.Tests.Utility
{
    public static class MockData
    {
        public static User AdminUser => new User()
        {
            Login = "vpupkin",
            Name = "Vasily Pupkin",
            PasswordHash = new byte[] { 1, 2, 3, 4 },
            PasswordSalt = new byte[] { 5, 6, 7, 8 }
        };

        public static User OperatorUser => new User()
        {
            Login = "itaranov",
            Name = "Ivan Taranov",
            PasswordHash = new byte[] { 4, 8, 15, 16 },
            PasswordSalt = new byte[] { 23, 42, 6, 6 }
        };

        public static ConnectionInfo TDMSConnectionInfo => new ConnectionInfo()
        {
            Name = "TDMS",
            AuthFieldNames = "TDMS field 1; TDMS field 2",
        };

        public static ConnectionInfo BimConnectionInfo => new ConnectionInfo()
        {
            Name = "BIM360",
            AuthFieldNames = "Bim field 1; Bim field 2",
        };

        public static IEnumerable<EnumDm> CreateEnumDms(string prefix, int connectionID, int count = 3)
        {
            for (int i = 0; i < count; i++)
                yield return new EnumDm() { Name = $"{prefix} EnumDm {i + 1}", ConnectionInfoID = connectionID };
        }

        public static IEnumerable<EnumDmValue> CreateEnumDmValues(int enumDmID, string prefix, int count = 3)
        {
            for (int i = 0; i < count; i++)
                yield return new EnumDmValue() { Value = $"{prefix} Value {i + 1}", EnumDmID = enumDmID };
        }
    }
}
