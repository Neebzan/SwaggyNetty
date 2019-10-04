using FileCheckerLib;
using GlobalVariablesLib;
using GlobalVariablesLib.Models;
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

namespace PatchManagerServer {
    public class PatchmanagerServer {
        public Dictionary<string, string> masterFiles = null;
        List<PatchClient> clients = new List<PatchClient>();
        string masterFilesPath = "";

        public PatchmanagerServer () {
            masterFilesPath = Path.GetFullPath(Path.Combine(@"..\..\..\..\", "GameClientBuild"));

            FileChecker.GetFilesDictionaryProgress += ProgressUpdateReceived;
            Task.Run(() => FileChecker.GetFilesDictionary(out masterFiles, masterFilesPath));

            while (masterFiles == null) {

            }
            Console.WriteLine("FilesDictionary created");

            Thread t = new Thread(HandleConnections);
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("Started listening for clients");
        }

        private void ProgressUpdateReceived (object sender, GetFilesDictionaryProgressEventArgs e) {
            Console.WriteLine("Progress {0}/{1}", e.ChecksumsGenerated, e.FilesFound);
        }

        private void HandleConnections () {
            TcpListener listener = new TcpListener(IPAddress.Any, GlobalVariables.PATCHMANAGER_PORT);
            listener.Start();
            while (true) {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Connected!");

                lock (clients) {
                    PatchClient patchClient = new PatchClient(client);
                    clients.Add(patchClient);
                    Thread t = new Thread(() => HandleTcpRequest(patchClient));
                    t.IsBackground = true;
                    t.Start();
                }

            }
        }

        private void HandleTcpRequest (PatchClient patchClient) {
            while (MessageFormatter.Connected(patchClient.client)) {
                try {
                    List<string> filesToDownload;
                    if (patchClient.client.GetStream().DataAvailable) {
                        if (patchClient.fileList == null) {
                            patchClient.fileList = JsonConvert.DeserializeObject<Dictionary<string, string>>(MessageFormatter.ReadStreamOnce(patchClient.client.GetStream()));
                            Console.WriteLine("Filelist received!");

                            Console.WriteLine("Comparing files to master list");
                            filesToDownload = FileChecker.CompareFileDictionaries(masterFiles, patchClient.fileList);

                            Console.WriteLine("Missing files on client:");
                            foreach (var item in filesToDownload) {
                                Console.WriteLine(item);
                            }

                            FileTransferModel fileTransferModel = GenerateFileTransferModel(filesToDownload, masterFilesPath);

                            Console.WriteLine("Sending missing files list to client");
                            byte [ ] modelData = MessageFormatter.MessageBytes(fileTransferModel);
                            patchClient.client.GetStream().Write(modelData, 0, modelData.Length);
                            Console.WriteLine("Files list sent");

                        }
                        else {
                            string fileToSend = MessageFormatter.ReadStreamOnce(patchClient.client.GetStream());
                            FileInfo fi = new FileInfo(masterFilesPath + '/' + fileToSend);
                            Console.WriteLine("{0} size: {1}", fi.Name, fi.Length);
                            byte [ ] preBuffer = BitConverter.GetBytes((int)fi.Length);
                            patchClient.client.Client.SendFile(fi.FullName, preBuffer, null, TransmitFileOptions.UseDefaultWorkerThread);
                            Console.WriteLine("{0} sent", fi.Name);
                        }
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            lock (clients)
                clients.Remove(patchClient);

            Console.WriteLine("{0} disconnected!", patchClient.client.Client.RemoteEndPoint.ToString());
            Console.WriteLine("Currently {0} other connected clients!", clients.Count);
        }

        private FileTransferModel GenerateFileTransferModel (List<string> filesToAdd, string directory = "") {
            FileTransferModel model = new FileTransferModel();

            foreach (var item in filesToAdd) {
                FileInfo t = new FileInfo(directory + '/' + item);
                model.Files.Add(new FileModel() { FilePath = item, Size = t.Length });
                model.TotalSize += t.Length;
            }

            model.RemainingSize += model.TotalSize;
            return model;
        }
    }
}
