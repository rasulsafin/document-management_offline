using System;
using System.Diagnostics;

namespace DocumentManagement.Contols
{
    internal class OpenHelper
    {
        internal static void Geany(string fileName)
        {
            Process.Start(@"c:\Program Files (x86)\Geany\bin\geany.exe", $"\"{fileName}\"");
        }
        internal static void Notepad(string fileName)
        {
            Process.Start("C:\\Windows\\System32\\notepad.exe", $"\"{fileName}\"") ;
        }
    }
}