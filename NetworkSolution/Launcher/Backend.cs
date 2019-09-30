using GlobalVariablesLib;
using GlobalVariablesLib.Models;
using PatchManagerClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher {
    public static class Backend {
        private static string middlewareIP = "10.131.68.42";
        private static int middlewarePort = 13010;
        public static UserModel loggedUser { get; private set; } = null;
        public static float PatchProgress = 0f;
        public static FileTransferModel PatchData = null;

        static Backend () {
            PatchmanagerClient.MissingFilesUpdated += PatchDataUpdated;
        }

        private static void PatchDataUpdated (object sender, EventArgs e) {
            if (PatchmanagerClient.MissingFiles.RemainingSize == 0) {
                PatchProgress = 100;
            }
            else 
                PatchProgress = ((1.0f - ((float)PatchmanagerClient.MissingFiles.RemainingSize / (float)PatchmanagerClient.MissingFiles.TotalSize)) * 100.0f);
        }

        public static void InitiatePatchClient () {
            Task t = new Task(() => PatchmanagerClient.StartPatchCheck(@"Downloads"));
            t.Start();
            PatchData = PatchmanagerClient.MissingFiles;

        }

        public static async Task<bool> SendLoginCredentials (string username, SecureString password) {
            TcpClient client = new TcpClient();

            string unsecurePassword = ConvertToUnsecureString(password);
            string hashedPassword = GetPasswordHash(unsecurePassword);

            try {
                await client.ConnectAsync(middlewareIP, middlewarePort);
            }

            catch (Exception) {
                return false;
            }



            GlobalVariablesLib.UserModel user = new GlobalVariablesLib.UserModel() { UserID = username, PswdHash = hashedPassword, RequestType = GlobalVariablesLib.RequestTypes.Get_User };

            byte [ ] msg = TcpHelper.MessageFormatter.MessageBytes<GlobalVariablesLib.UserModel>(user);

            try {
                client.GetStream().Write(msg, 0, msg.Length);
            }
            catch (Exception) {
                return false;
            }

            while (true) {
                string result = TcpHelper.MessageFormatter.ReadStreamOnce(client.GetStream());
                if (!string.IsNullOrEmpty(result)) {
                    UserModel resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(result);
                    if (resultUser.Status == RequestStatus.Success) {
                        if (resultUser.RequestType == RequestTypes.Token_Get) {
                            loggedUser = resultUser;
                            return true;
                        }
                    }
                    else {
                        loggedUser = null;
                        return false;
                    }
                }
            }
        }

        public static void Logout () {
            loggedUser = null;
        }

        public static async Task<bool> SendRegisterRequest (string username, SecureString password) {
            TcpClient client = new TcpClient();

            string unsecurePassword = ConvertToUnsecureString(password);
            string hashedPassword = GetPasswordHash(unsecurePassword);

            try {

                await client.ConnectAsync(middlewareIP, middlewarePort);
            }
            catch (Exception) {
                return false;
            }

            GlobalVariablesLib.UserModel user = new GlobalVariablesLib.UserModel() { UserID = username, PswdHash = hashedPassword, RequestType = GlobalVariablesLib.RequestTypes.Create_User };

            byte [ ] msg = TcpHelper.MessageFormatter.MessageBytes<GlobalVariablesLib.UserModel>(user);

            try {
                client.GetStream().Write(msg, 0, msg.Length);
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        public static void LaunchGame () {
            Process.Start("F:/Steam/steamapps/common/Cube World/cubeworld.exe");
        }

        private static string GetPasswordHash (string password) {

            using (HMACSHA512 t = new HMACSHA512(Encoding.UTF8.GetBytes(password))) {
                byte [ ] hash;
                hash = t.ComputeHash(Encoding.UTF8.GetBytes(password));
                password = BitConverter.ToString(hash).Replace("-", "");
            }

            return password;
        }
        private static string ConvertToUnsecureString (SecureString securePassword) {
            if (securePassword == null) {
                return string.Empty;
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private static void ReadForAnswer () {
            //TcpHelper.MessageFormatter.ReadForMessages(new NetworkStre));
        }

        public static bool CheckPassUniformity (SecureString _password, SecureString _confirmPass) {
            string pass = ConvertToUnsecureString(_password);
            string confirmPass = ConvertToUnsecureString(_confirmPass);

            if (pass == confirmPass) {
                return true;
            }
            else {
                return false;
            }
        }


    }
}
