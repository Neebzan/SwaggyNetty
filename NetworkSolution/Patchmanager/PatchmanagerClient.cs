using FileCheckerLib;
using GlobalVariablesLib;
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

        string downloadDirectory = "GameFiles";

        public PatchmanagerClient()
        {
            if (downloadDirectory != "")
                downloadDirectory += "\\";

            FileChecker.GetFilesDictionaryProgress += ProgressUpdateReceived;
            Task.Run(() => FileChecker.GetFilesDictionary(out allFilesDictionary, downloadDirectory));

            while (allFilesDictionary == null)
            {

            }
            Console.WriteLine("FilesDictionary created");
            Console.WriteLine("Sending FilesDictionary to server");
            while (client == null)
            {
                try
                {
                    //client = new TcpClient("178.155.161.248", 14000);
                    client = new TcpClient("127.0.0.1", 14000);
                }
                catch
                {

                }
            }

            SendFileDictionaryToServer();

            FileTransferModel missingFiles = null;
            bool completed = false;

            bool waitingForFile = false;

            while (!completed)
            {
                if (MessageFormatter.Connected(client))
                {
                    if (missingFiles == null)
                    {
                        while (client.GetStream().DataAvailable)
                        {

                            Console.WriteLine("Awaiting missing files list from Patch Server");
                            string jsonList = MessageFormatter.ReadMessage(client.GetStream());
                            missingFiles = JsonConvert.DeserializeObject<FileTransferModel>(jsonList);
                            Console.WriteLine("Missing files list received");
                            Console.WriteLine("{0} files missing in total", missingFiles.Files.Count);
                            Console.WriteLine("Total size of missing files: {0}bytes", missingFiles.TotalSize);
                            foreach (var item in missingFiles.Files)
                            {
                                Console.WriteLine("Missing: {0}", item.FilePath);
                            }
                            if (missingFiles.Files.Count == 0)
                                completed = true;
                        }
                    }
                    //Start requesting missing files from server
                    else if (missingFiles.Files.Count > 0)
                    {
                        if (!waitingForFile)
                        {
                            byte[] fileRequestData = MessageFormatter.MessageBytes(missingFiles.Files[0].FilePath);
                            client.GetStream().Write(fileRequestData, 0, fileRequestData.Length);
                            waitingForFile = true;
                        }
                        else
                        {
                            while (client.GetStream().DataAvailable)
                            {
                                MessageFormatter.ReadFile(client, missingFiles.Files[0].FilePath, downloadDirectory);
                                missingFiles.Files.RemoveAt(0);
                                waitingForFile = false;
                            }
                        }
                        if (missingFiles.Files.Count == 0)
                        {
                            waitingForFile = false;
                            completed = true;
                        }
                    }

                }
            }

            Console.WriteLine("ALL DONE");
            Console.ReadKey();

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
