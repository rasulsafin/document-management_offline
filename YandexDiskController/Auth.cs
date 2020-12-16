using DocumentManagement.Connection.YandexDisk;

namespace DocumentManagement
{
    internal partial class MainViewModel
    {
        public static string AccessToken { get; private set; }

        static class Auth
        {
            private const string CLIENT_ID = "b1a5acbc911b4b31bc68673169f57051";
            private const string CLIENT_Secret = "b4890ed3aa4e4a4e9e207467cd4a0f2c";

            public static async void StartAuth()
            {
                YandexDiskAuth auth = new YandexDiskAuth();
                var result = await auth.GetDiskSdkToken();
                //if (result.Error == null)
                //{
                    MainViewModel.AccessToken = result;
                    controller = new YandexDiskController(AccessToken);
                    //var items = await controller.GetListAsync("/");
                    //controller.GetList2Async(callbackGetList, "/");
                    //MainViewModel.Instanse.SetFolderItems(items);
                //}
                //else
                //{
                //    MessageBox.Show($" { result.Error.Message}", "Ошибка");
                //}
            }

            //private static void callbackGetList(GenericSdkEventArgs<IEnumerable<DiskItemInfo>> obj)
            //{
            //    if (obj.Error == null)
            //        Instanse.SetFolderItems(obj.Result);
            //    else
            //    {
            //        MessageBox.Show($" { obj.Error.Message}", "Ошибка");
            //    }
            //}
        }

        
    }
}