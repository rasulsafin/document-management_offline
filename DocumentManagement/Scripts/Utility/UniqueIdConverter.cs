using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace MRS.Bim.Tools
{
    public static class UniqueIdConverter 
    {
        [DllImport("IfcGuide")]
        static extern void getString64FromGuid(ref Guid guid, StringBuilder s64);
        [DllImport("IfcGuide")]
        static extern bool getGuidFromString64(string s64, ref Guid guid);

        private static Guid String64ToGuid(string s64)
        {
            Guid guid = new Guid();
            getGuidFromString64(s64.ToString(), ref guid);

            return guid;
        }
        private static string GuidToString64(Guid guid)
        {
            StringBuilder s = new StringBuilder("                        ");
            getString64FromGuid(ref guid, s);
            
            return s.ToString();
        }

        public static string UniqueIdToGuid(string uniqueId)
        {
            int elementId = int.Parse(uniqueId.Substring(37), NumberStyles.AllowHexSpecifier);
            int last32bits = int.Parse(uniqueId.Substring(28, 8), NumberStyles.AllowHexSpecifier);
            int xor = last32bits ^ elementId;

            var dwf = uniqueId.Substring(0, 28) + xor.ToString("x8");

            var guid = new Guid(dwf);

            return GuidToString64(guid);
        }
    }
}
