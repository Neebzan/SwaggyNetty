using FileCheckerLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpHelper;

namespace PatchManagerServer
{
    public class PatchmanagerServer
    {
        public Dictionary<string, string> masterFiles = null;
        List<PatchClient> clients = new List<PatchClient>();

        public PatchmanagerServer()
        {
            FileChecker.GetFilesDictionaryProgress += ProgressUpdateReceived;
            Task.Run(() => FileChecker.GetFilesDictionary(out masterFiles));

            while (masterFiles == null)
            {

            }
            Console.WriteLine("FilesDictionary created");

            Thread t = new Thread(HandleConnections);
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("Started listening for clients");
        }

        private void ProgressUpdateReceived(object sender, GetFilesDictionaryProgressEventArgs e)
        {
            Console.WriteLine("Progress {0}/{1}", e.ChecksumsGenerated, e.FilesFound);
        }

        private void HandleConnections()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 14000);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Connected!");

                lock (clients)
                {
                    PatchClient patchClient = new PatchClient(client);
                    clients.Add(patchClient);
                    Thread t = new Thread(() => HandleTcpRequest(patchClient));
                    t.IsBackground = true;
                    t.Start();
                }

            }
        }

        private void HandleTcpRequest(PatchClient patchClient)
        {
            while (MessageFormatter.Connected(patchClient.client))
            {
                List<string> filesToDownload;
                if (patchClient.client.GetStream().DataAvailable)
                {
                    if (patchClient.fileList == null)

                        patchClient.fileList = JsonConvert.DeserializeObject<Dictionary<string, string>>(MessageFormatter.ReadMessage(patchClient.client.GetStream()));
                    Console.WriteLine("Filelist received!");

                    Console.WriteLine("Comparing files to master list");
                    filesToDownload = FileChecker.CompareFileDictionaries(masterFiles, patchClient.fileList);

                    Console.WriteLine("Missing files on client:");
                    foreach (var item in filesToDownload)
                    {
                        Console.WriteLine(item);
                    }

                    Console.WriteLine("Sending a test file");
                    string filePath = "Vedlaeg.zip";
                    FileInfo fi = new FileInfo(filePath);
                    Console.WriteLine("Test file size: {0}", fi.Length);
                    byte[] preBuffer = BitConverter.GetBytes((int)fi.Length);
                    patchClient.client.Client.SendFile(filePath, preBuffer, null, TransmitFileOptions.UseDefaultWorkerThread);
                    Console.WriteLine("Test file send");



                }
            }

            lock (clients)
                clients.Remove(patchClient);

            Console.WriteLine("{0} disconnected!", patchClient.client.Client.RemoteEndPoint.ToString());
            Console.WriteLine("Currently {0} other connected clients!", clients.Count);
        }
    }
}
