using FileCheckerLib;
using GlobalVariablesLib;
using GlobalVariablesLib.Models;
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

namespace PatchManagerClient {

    public enum PatchStatus { Connecting, Downloading, Done, CheckingFiles};

    public static class PatchmanagerClient {
        public static Dictionary<string, string> allFilesDictionary = null;
        public static FileTransferModel MissingFiles = null;
        static TcpClient client;
        public static PatchStatus Status = PatchStatus.Connecting;


        static string downloadDirectory = @"";

        public static EventHandler MissingFilesUpdated = delegate { };
        public static EventHandler DownloadComplete = delegate { };
        public static EventHandler StatusChanged = delegate { };


        public static void StartPatchCheck (string dir) {
            Status = PatchStatus.Connecting;
            StatusChanged.Invoke(null, new EventArgs());
            downloadDirectory = dir;
            FileChecker.GetFilesDictionaryProgress += ProgressUpdateReceived;
            Task.Run(() => FileChecker.GetFilesDictionary(out allFilesDictionary, downloadDirectory));

            while (allFilesDictionary == null) {

            }
            while (client == null) {
                try {
                    //client = new TcpClient("178.155.161.248", 14000);
                    client = new TcpClient(GlobalVariables.PATCHMANAGER_IP, GlobalVariables.PATCHMANAGER_PORT);
                }
                catch {

                }
            }



            SendFileDictionaryToServer();
            MissingFiles = null;
            bool completed = false;

            bool waitingForFile = false;

            Status = PatchStatus.Downloading;
            StatusChanged.Invoke(null, new EventArgs());

            while (!completed) {
                if (MessageFormatter.Connected(client)) {
                    if (MissingFiles == null) {
                        while (client.GetStream().DataAvailable) { 
                            string jsonList = MessageFormatter.ReadStreamOnce(client.GetStream());
                            MissingFiles = JsonConvert.DeserializeObject<FileTransferModel>(jsonList);
                            MissingFilesUpdated.Invoke(null, new EventArgs());
                            if (MissingFiles.Files.Count == 0) {
                                DownloadComplete.Invoke(null, new EventArgs());
                                Status = PatchStatus.Done;
                                StatusChanged.Invoke(null, new EventArgs());
                                completed = true;
                            }
                        }
                    }
                    //Start requesting missing files from server
                    else if (MissingFiles.Files.Count > 0) {
                        if (!waitingForFile) {
                            byte [ ] fileRequestData = MessageFormatter.MessageBytes(MissingFiles.Files [ 0 ].FilePath);
                            client.GetStream().Write(fileRequestData, 0, fileRequestData.Length);
                            waitingForFile = true;
                        }
                        else {
                            while (client.GetStream().DataAvailable) {
                                MessageFormatter.ReadFile(client, MissingFiles.Files [ 0 ].FilePath, downloadDirectory);
                                MissingFiles.RemainingSize -= MissingFiles.Files[0].Size;
                                MissingFilesUpdated.Invoke(null, new EventArgs());
                                MissingFiles.Files.RemoveAt(0);
                                waitingForFile = false;
                            }
                        }
                        if (MissingFiles.Files.Count == 0) {
                            waitingForFile = false;
                            DownloadComplete.Invoke(null, new EventArgs());
                            Status = PatchStatus.Done;
                            StatusChanged.Invoke(null, new EventArgs());
                            completed = true;
                        }
                    }

                }
            }
        }

        public static void SendFileDictionaryToServer () {
            byte [ ] data = MessageFormatter.MessageBytes(allFilesDictionary, Formatting.Indented);
            client.GetStream().Write(data, 0, data.Length);
        }

        private static void ProgressUpdateReceived (object sender, GetFilesDictionaryProgressEventArgs e) {
        }
    }
}
