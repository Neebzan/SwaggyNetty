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
            //client = new TcpClient("178.155.161.248", 14000);
            while (client == null)
            {
                try
                {
                    client = new TcpClient("127.0.0.1", 14000);
                }
                catch
                {

                }
            }

            SendFileDictionaryToServer();

            while (true)
            {
                if (MessageFormatter.Connected(client))
                {
                    byte[] readBuffer = new byte[4];
                    while (client.GetStream().DataAvailable)
                    {
                        int bytesRead = 0;

                        while (bytesRead < 4)
                        {
                            bytesRead += client.GetStream().Read(readBuffer, bytesRead, 4 - bytesRead);
                        }

                        int totalFileSize = BitConverter.ToInt32(readBuffer, 0);

                        Console.WriteLine("Size of incoming file {0}", totalFileSize);

                        using (var output = File.Create("TEST.zip"))
                        {
                            Console.WriteLine("Client connected. Starting to receive the file");

                            // read the file in chunks of 1KB
                            var buffer = new byte[1024];
                            bytesRead = 0;
                            int totalBytesRead = 0;
                            while (totalBytesRead < totalFileSize)
                            {
                                Console.WriteLine("Size of incoming file {0}", totalFileSize);
                                bytesRead = client.Client.Receive(buffer, buffer.Length, SocketFlags.None);
                                output.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                Console.WriteLine("{0} bytes read", totalBytesRead);
                            }
                            Console.WriteLine("File received!");
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
