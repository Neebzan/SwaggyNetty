using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Helpers;

namespace Login_Middleware
{
    public class Database_Request_Handler
    {
        public string UserDataAddress { get; set; }

        public string Request_Get(DynamicJsonObject data)
        {
            string responseOutput = string.Empty;

            dynamic dataObj = data;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UserDataAddress+$"//{dataObj.UserID}");

            request.Method = "GET";

            HttpWebResponse httpResponse = null;

            try
            {
                httpResponse = (HttpWebResponse)request.GetResponse();

                using (Stream responseStream = httpResponse.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            responseOutput = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR! Exception Encountered!\nException Message: {e.ToString()}\n TLDR: {e.Message}");
            }
            finally
            {
                if (httpResponse != null)
                {
                    ((IDisposable)httpResponse).Dispose();
                }
            }

            return responseOutput;
        }
        public void Request_Post(DynamicJsonObject data)
        {
            string responseOutput = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UserDataAddress);
            request.Method = "POST";

            string dataEncoded = Json.Encode(data);

            byte[] byteArray = Encoding.UTF8.GetBytes(dataEncoded);

            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = byteArray.Length;

        }

        public void Request_Put()
        {

        }

        public void Request_Delete()
        {

        }



    }
}
