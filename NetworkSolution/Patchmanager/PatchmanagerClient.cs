using FileCheckerLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpHelper;

namespace PatchmanagerClient
{
    public class PatchmanagerClient
    {
        public Dictionary<string, string> allFilesDictionary = null;

        TcpClient client;

        public PatchmanagerClient()
        {
            
            FileChecker.GetFilesDictionaryProgress += ProgressUpdateReceived;
            Task.Run(() => FileChecker.GetFilesDictionary(out allFilesDictionary));

            while (allFilesDictionary == null)
            {

            }
            Console.WriteLine("FilesDictionary created");
            Console.WriteLine("Sending FilesDictionary to server");
            client = new TcpClient("127.0.0.1", 14000);
            SendFileDictionaryToServer();

            while (true)
            {
                if (MessageFormatter.Connected(client))
                {
                    using (var output = File.Create("TESTFILE.exe"))
                    {
                        Console.WriteLine("Client connected. Starting to receive the file");

                        // read the file in chunks of 1KB
                        var buffer = new byte[1024];
                        int bytesRead;
                        while ((bytesRead = client.Client.Receive(buffer, buffer.Length, SocketFlags.None)) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                            Console.WriteLine("{0} bytes read", bytesRead);
                        }
                    }
                }
            }
        }

        public void SendFileDictionaryToServer()
        {
            byte[] data = MessageFormatter.MessageBytes(allFilesDictionary, Formatting.Indented);
            client.GetStream().Write(data, 0, data.Length);
            Console.WriteLine("FilesDictionary send");
        }

        private void ProgressUpdateReceived(object sender, GetFilesDictionaryProgressEventArgs e)
        {
            Console.WriteLine("Progress {0}/{1}", e.ChecksumsGenerated, e.FilesFound);
        }
    }
}
