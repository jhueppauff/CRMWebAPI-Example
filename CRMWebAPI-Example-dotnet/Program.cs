using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CRMWebAPI_Example
{
    class Program
    {
        private const string clientId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        private const string crmUrl = "https://crm.example.org/api/data/v8.2/";
        private const string redirectUri = "https://crm.example.org";

        private static string username = "";
        private static string password = "";

        private const string adfsTokenURL = "https://adfs.example.org/adfs/oauth2/token";

        private static HttpClient client;

        static async Task Main()
        {
            client = new();

            FillUsernameAndPassword();

            // Request Token from ADFS
            string authToken = await RequestAuthToken();

            if (!string.IsNullOrEmpty(authToken))
            {
                CallWhoAmIRequest(authToken);
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static async Task CallWhoAmIRequest(string authToken)
        {
            const string releativePath = "WhoAmI";

            Uri uri = new(crmUrl + releativePath);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, adfsTokenURL);
            requestMessage.Headers.Add("Content-Type", "application/json");
            requestMessage.Headers.Add("cache-control", "no-cache");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.SendAsync(requestMessage);

            try
            {
                Stream data = await response.Content.ReadAsStreamAsync();

                StreamReader reader = new(data);

                Console.Write("Answer from Server:");
                Console.WriteLine(reader.ReadToEnd());

                reader.Close();
                data.Close();
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

        private static async Task<string> RequestAuthToken()
        {
            string body = $"resource={crmUrl}&client_id={clientId}&grant_type=password&username={username}&password={password}&scope=openid&redirect_uri={redirectUri}";
            ASCIIEncoding encoding = new();
            byte[] requestBody = encoding.GetBytes(body);

            Uri uri = new(adfsTokenURL);
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, adfsTokenURL);
            requestMessage.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            requestMessage.Headers.Add("cache-control", "no-cache");

            var response = await client.SendAsync(requestMessage);


            Stream stream = await response.Content.ReadAsStreamAsync();
            stream.Write(requestBody, 0, requestBody.Length);

            try
            {
                StreamReader reader = new(stream);

                var jResponse = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(reader.ReadToEnd());

                reader.Close();
                stream.Close();

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
