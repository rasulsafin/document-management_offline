using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace DocumentManagement.Connection.YandexDisk
{
    public static class YandexHelper
    {
        private static readonly string URI_API_YANDEX = "https://webdav.yandex.ru/";
        #region Multu Platform Open Browser
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public static HttpWebRequest RequestGetList(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Method = "PROPFIND";
            request.Accept = "*/*";
            request.Headers["Depth"] = "1";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }

        public static HttpWebRequest RequestCreateDir(string accessToken, string pathNewDirectory)
        {
            var url = URI_API_YANDEX + pathNewDirectory;
            var request = WebRequest.CreateHttp(url);
            request.Method = "MKCOL";
            request.Accept = "*/*";            
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }

        public static string NewDirectory(string path, string nameDir)
        {
            List<string> items = new List<string>(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            items.Add(nameDir);
            string result = string.Join('/', items);
            return $"/{result}/";
        }

        public static string NewFile(string path, string nameFile)
        {
            List<string> items = new List<string>(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            items.Add(nameFile);
            string result = string.Join('/', items);
            return $"/{result}";
        }

        public static HttpWebRequest RequestDownloadFile(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Host = "webdav.yandex.ru";
            request.Method = "GET";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }

        internal static HttpWebRequest RequestLoadFile(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Host = "webdav.yandex.ru";
            request.Method = "PUT";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;

            return request;
        }

        internal static HttpWebRequest RequestDelete(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Host = "webdav.yandex.ru";
            request.Method = "DELETE";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }
        #endregion
    }

    #region LOGGER
    public class CoolLogger
    {
        private FileInfo logFile;
        //private StreamWriter log;

        public CoolLogger(string name)
        {
            logFile = new FileInfo($"{name}.log");
            using (var log = logFile.AppendText())
            {
                log.WriteLine("\n=== Начало логгирования ===");
                log.WriteLine("date : " + System.DateTime.Now.ToString("dd.MM.yy HH:mm:ss.FFF"));
            }
        }

        public void Message(string message = "",
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            using (var log = logFile.AppendText())
            {
                log.Write($"\n{memberName}[{sourceLineNumber}]");
                if (!string.IsNullOrWhiteSpace(message)) log.Write($": " + message);
            }
        }

        public void Save()
        {
            //logFile
        }

        public void Error(System.Exception ex)
        {
            // Get stack trace for the exception with source file information
            var st = new System.Diagnostics.StackTrace(ex, true);
            // Get the top stack frame
            var frame = st.GetFrame(0);
            // Get the line number from the stack frame
            var line = frame.GetFileLineNumber();

            using (var log = logFile.AppendText())
            {
                log.WriteLine($"\nError<{ex.GetType().Name}>:{ex.Message}");
                log.WriteLine($"Source:{ex.Source};"
                    + $"TargetSite.Name:{ex.TargetSite.Name};"
                    + $"HResult:{ex.HResult};"
                    + $"line:{line};"
                    );
                log.WriteLine($"StackTrace:{ex.StackTrace}");
            }
        }

        internal void Clear()
        {
            logFile.Delete();
        }

        public void Open()
        {
            //System.Diagnostics.Process.Start(logFile.FullName);
            Process.Start("C:\\Windows\\System32\\notepad.exe", logFile.FullName.Trim());
        }
    }
    #endregion

}
