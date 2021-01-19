using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MRS.DocumentManagement.Connection.YandexDisk;

namespace MRS.DocumentManagement
{
    public static class Auth
    {
        private const string CLIENT_ID = "b1a5acbc911b4b31bc68673169f57051";
        private const string CLIENT_Secret = "b4890ed3aa4e4a4e9e207467cd4a0f2c";
        public static List<Action<string>> LoadActions = new List<Action<string>>();

        public static string AccessToken { get; private set; }

        public static async void StartAuth()
        {
            YandexDiskAuth auth = new YandexDiskAuth();
            var result = await auth.GetDiskSdkToken();
            AccessToken = result;
        }

        internal static async void Loaded(object sender, RoutedEventArgs e)
        {
            while (AccessToken == null)
                await Task.Delay(100);

            foreach (var action in LoadActions)
            {
                action.Invoke(AccessToken);
            }
        }
    }
}
