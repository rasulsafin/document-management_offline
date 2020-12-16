using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace DocumentManagement.Connection.YandexDisk
{
    public class YandexDiskController
    {

        static void Log
        

        private string accessToken;
        

        public YandexDiskController(string accessToken)
        {
            this.accessToken = accessToken;

        }

        #region GetList
        public async Task<IEnumerable<DiskElement>> GetListAsync(string path = "/")
        {

            var request = YandexHelper.RequestGetList(accessToken, path);

            WebResponse response = await request.GetResponseAsync();
            Console.WriteLine("Ответ получен...");
            XmlDocument xml = new XmlDocument();
            using (Stream stream = response.GetResponseStream())
            {
                XmlReaderSettings settings = new XmlReaderSettings()
                {

                };


                //using (StreamReader reader = new StreamReader(stream))
                //{
                //    //for (int i = 0; i < 2; i++)
                //{
                //string text = reader.ReadToEnd();
                //var xmlBytes = Encoding.UTF8.GetBytes(text);
                //using (var xmlStream = new MemoryStream(xmlBytes))
                //{
                using (var xmlReader = XmlReader.Create(stream))
                    xml.Load(xmlReader);


                //XmlReader.Create(stream);
                //}
                //Console.WriteLine(text);
                //}

                //string xmlStr = Encoding.UTF8.
                //var xmlBytes = Encoding.UTF8.GetBytes(responseText);
                //xml.Load(xmlBytes);
                //File.WriteAllText("response.xml", respons);
                //}
                //xml.Load(stream);
            }
            response.Close();
            Console.WriteLine("Запрос выполнен...");
            List<DiskElement> items = DiskElement.GetElements(xml.DocumentElement);
            return items;
        } 
        #endregion

        
    }
}
