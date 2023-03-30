using System;
using System.Net;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable SYSLIB0014
namespace Geoguessr.Modules
{
    internal class Utilities
    {
        readonly private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


        // SET UP A SETTING
        public static void CheckSetting(ref bool setting, string settingName, bool defaultValue = false)
        {
            if (File.Exists(@$"{appData}\Geoguessr\Settings\{settingName}.geoguessr"))
            {
                if (bool.TryParse(File.ReadAllText(@$"{appData}\Geoguessr\Settings\{settingName}.geoguessr"), out bool ts))
                {
                    setting = ts;
                }
                else
                {
                    File.Delete(@$"{appData}\Geoguessr\Settings\{settingName}.geoguessr");
                }
            }
            else
            {
                File.WriteAllText(@$"{appData}\Geoguessr\Settings\{settingName}.geoguessr", defaultValue.ToString());
                setting = defaultValue;
            }
        }

        // LOG IN THE CONSOLE
        public static void Log(string log, ConsoleColor importance = ConsoleColor.DarkGreen, bool viewLog = true)
        {
            ConsoleColor old = Console.ForegroundColor;

            Console.ForegroundColor = importance;
            if (viewLog) { Console.Write("[LOG] - "); }
            Console.WriteLine(log);
            Console.ForegroundColor = old;
        }

        // SEND A "GET" WEB REQUEST
        public static string Get(string web, Dictionary<string, string>? headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web);
            request.ContentType = contentType;
            if (headers != null)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    request.Headers.Add(headers.ElementAt(i).Key, headers.ElementAt(i).Value);
                }
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                using WebResponse? httpResponse = e.Response;
                response = (HttpWebResponse)httpResponse;
                try
                {
                    using StreamReader reader = new(response.GetResponseStream());
                    return reader.ReadToEnd();
                }
                catch
                {
                    return e.Message;
                }
            }
            using StreamReader streamReader = new(response.GetResponseStream());
            return streamReader.ReadToEnd();
        }
        
        // GETS THE STATUS CODE OF A "GET" WEB REQUEST
        public static HttpStatusCode GetSC(string web, Dictionary<string, string>? headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(web);
            request.ContentType = contentType;
            if (headers != null)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    request.Headers.Add(headers.ElementAt(i).Key, headers.ElementAt(i).Value);
                }
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                using WebResponse? httpResponse = e.Response;
                response = (HttpWebResponse)httpResponse;
                return response.StatusCode;
            }
            return response.StatusCode;
        }
    }
}