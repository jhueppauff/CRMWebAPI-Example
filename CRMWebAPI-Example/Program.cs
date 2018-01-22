using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace CRMWebAPI_Example
{
    class Program
    {
        private static string clientId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        private static string crmUrl = "https://crm.example.org/api/data/v8.2/";
        private static string redirectUri = "https://crm.example.org";

        private static string username = "";
        private static string password = "";

        private static string adfsTokenURL = "https://adfs.example.org/adfs/oauth2/token";

        static void Main(string[] args)
        {
            FillUsernameAndPassword();

            // Request Token from ADFS
            string authToken = RequestAuthToken();

            if (!string.IsNullOrEmpty(authToken))
            {
                CallWhoAmIRequest(authToken);
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void CallWhoAmIRequest(string authToken)
        {
            string releativePath = "WhoAmI";

            Uri uri = new Uri(crmUrl + releativePath);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Headers.Add("cache-control", "no-cache");
            request.Headers.Add("Authorization", "Bearer " + authToken);
            request.ContentType = "application/json";

            try
            {
                var response = request.GetResponse();

                Stream data = response.GetResponseStream();

                StreamReader reader = new StreamReader(data);

                Console.Write("Answer from Server:");
                Console.WriteLine(reader.ReadToEnd());

                reader.Close();
                response.Close();
            }
            catch (WebException wex)
            {
                if (wex.Response is HttpWebResponse httpResponse)
                {
                    throw new ApplicationException(string.Format(
                        "Remote server call {0} {1} resulted in a http error {2} {3}.",
                        GetCurrentMethod(),
                        uri,
                        httpResponse.StatusCode,
                        httpResponse.StatusDescription), wex);
                }
                else
                {
                    throw new ApplicationException(string.Format(
                        "Remote server call {0} {1} resulted in an error.",
                        GetCurrentMethod(),
                        uri), wex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(0);
            }
        }

        private static string RequestAuthToken()
        {
            string body = $"resource={crmUrl}&client_id={clientId}&grant_type=password&username={username}&password={password}&scope=openid&redirect_uri={redirectUri}";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] requestBody = encoding.GetBytes(body);

            Uri uri = new Uri(adfsTokenURL);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("cache-control", "no-cache");

            request.ContentLength = requestBody.Length;

            Stream stream = request.GetRequestStream();
            stream.Write(requestBody, 0, requestBody.Length);

            try
            {
                var response = request.GetResponse();

                Stream data = response.GetResponseStream();

                StreamReader reader = new StreamReader(data);

                var jResponse = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(reader.ReadToEnd());

                reader.Close();
                response.Close();

                return jResponse["access_token"].ToString();
            }
            catch (WebException wex)
            {
                if (wex.Response is HttpWebResponse httpResponse)
                {
                    throw new ApplicationException(string.Format(
                        "Remote server call {0} {1} resulted in a http error {2} {3}.",
                        GetCurrentMethod(),
                        uri,
                        httpResponse.StatusCode,
                        httpResponse.StatusDescription), wex);
                }
                else
                {
                    throw new ApplicationException(string.Format(
                        "Remote server call {0} {1} resulted in an error.",
                        GetCurrentMethod(),
                        uri), wex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(0);
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        #region Fill Username and Password
        private static void FillUsernameAndPassword()
        {
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Please enter your username");
                username = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(password))
            {
                Console.Write("Enter your password: ");
                ConsoleKeyInfo key;

                do
                {
                    key = Console.ReadKey(true);

                    // Backspace Should Not Work
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        password += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                        {
                            password = password.Substring(0, (password.Length - 1));
                            Console.Write("\b \b");
                        }
                    }
                }
                // Stops Receving Keys Once Enter is Pressed
                while (key.Key != ConsoleKey.Enter);
            }
        }
        #endregion
    }
}
