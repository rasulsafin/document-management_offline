using System.Runtime.CompilerServices;

namespace MRS.Bim.Tools
{
    internal static class PreferenceHandler
    {
        public static T Get<T>(string id, ref T set)
        {
            if (set != null)
                return set;
            return set = (T) BimEnvironment.Instance.GetSettingsFunc(id, typeof(T));
        }

        public static void Set<T>(string id, ref T set, T value)
        {
            set = value;
            BimEnvironment.Instance.SaveSettingsAction(id, value);
        }

        public static string CreateKeyByMethodName(string prefix, [CallerMemberName] string method = null)
            => $"{prefix}.{method}";
    }
}