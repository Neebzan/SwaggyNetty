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
    internal class BackendErrorEventArgs : EventArgs {
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }

        public BackendErrorEventArgs (string _errorTitle, string _errorMessage) {
            ErrorTitle = _errorTitle;
            ErrorMessage = _errorMessage;
        }
    }

    internal static class Backend {
        private static string middlewareIP = "10.131.68.126";
        private static int middlewarePort = 13010;
        public static UserModel loggedUser { get; private set; } = null;
        public static float PatchProgress = 0f;
        public static float ConnectionTimeoutMS = 5000.0f;
        public static FileTransferModel PatchData = null;

        public static EventHandler<BackendErrorEventArgs> BackendErrorEncountered;

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

        private static TcpClient ConnectoToMiddleware (float timeout) {
            TcpClient client = new TcpClient();

            try {
                var result = client.BeginConnect(middlewareIP, middlewarePort,null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout));
                if (success) {
                    client.EndConnect(result);
                    return client;
                }
                else {
                    client.Dispose();
                    BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Client connection failed", "The client connection timed-out after " + timeout.ToString() + " ms"));
                    return null;
                }
            }
            catch (Exception e) {
                client.Dispose();
                BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Client connection failed", e.Message));
                return null;
            }
        }

        private static async Task<bool> WriteToMiddleware(TcpClient client, byte[] msg) {
            try {
                await client.GetStream().WriteAsync(msg, 0, msg.Length);
                return true;
            }
            catch (Exception e) {
                client.Dispose();
                BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Client failed to send request", e.Message));
                return false;
            }
        }

        public static async Task<bool> SendLoginCredentials (string username, SecureString password) {
            string unsecurePassword = ConvertToUnsecureString(password);
            string hashedPassword = GetPasswordHash(unsecurePassword);

            TcpClient client = ConnectoToMiddleware(ConnectionTimeoutMS);

            if (client == null)
                return false;
            

            GlobalVariablesLib.UserModel user = new GlobalVariablesLib.UserModel() { UserID = username, PswdHash = hashedPassword, RequestType = GlobalVariablesLib.RequestTypes.Get_User };

            byte [ ] msg = TcpHelper.MessageFormatter.MessageBytes<GlobalVariablesLib.UserModel>(user);

            if (!await WriteToMiddleware(client, msg))
                return false;

            while (true) {
                string result = TcpHelper.MessageFormatter.ReadStreamOnce(client.GetStream());
                if (!string.IsNullOrEmpty(result)) {
                    UserModel resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(result);
                    if (resultUser.Status == RequestStatus.Success) {
                        if (resultUser.RequestType == RequestTypes.Token_Get) {
                            loggedUser = resultUser;
                            client.Dispose();
                            return true;
                        }
                    }
                    else {
                        loggedUser = null;
                        client.Dispose();
                        return false;
                    }
                }
            }
        }

        public static async Task<bool> SendTokenLogin (string token, string userID) {
            TcpClient client = ConnectoToMiddleware(ConnectionTimeoutMS);

            if (client == null)
                return false;

            GlobalVariablesLib.UserModel user = new GlobalVariablesLib.UserModel() { UserID = userID, Token = token, RequestType = GlobalVariablesLib.RequestTypes.Token_Check };

            byte [ ] msg = TcpHelper.MessageFormatter.MessageBytes<GlobalVariablesLib.UserModel>(user);

            if (!await WriteToMiddleware(client, msg))
                return false;

            while (true) {
                string result = TcpHelper.MessageFormatter.ReadStreamOnce(client.GetStream());
                if (!string.IsNullOrEmpty(result)) {
                    UserModel resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(result);
                    if (resultUser.Status == RequestStatus.Success) {
                        if (resultUser.TokenResponse == TokenResponse.Valid) {
                            loggedUser = resultUser;
                            client.Dispose();
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        loggedUser = null;
                        client.Dispose();
                        return false;
                    }
                }
            }
        }

        public static void Logout () {
            loggedUser = null;
        }

        public static async Task<bool> SendRegisterRequest (string username, SecureString password) {
            string unsecurePassword = ConvertToUnsecureString(password);
            string hashedPassword = GetPasswordHash(unsecurePassword);

            TcpClient client = ConnectoToMiddleware(ConnectionTimeoutMS);

            if (client == null)
                return false;

            UserModel user = new UserModel() { UserID = username, PswdHash = hashedPassword, RequestType = RequestTypes.Create_User };

            byte [ ] msg = TcpHelper.MessageFormatter.MessageBytes<UserModel>(user);

            if (!await WriteToMiddleware(client, msg))
                return false;


            while (true) {
                string result = TcpHelper.MessageFormatter.ReadStreamOnce(client.GetStream());
                if (!string.IsNullOrEmpty(result)) {
                    UserModel resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(result);
                    if (resultUser.Status == RequestStatus.Success) {
                        if (resultUser.RequestType == RequestTypes.Create_User) {
                            loggedUser = resultUser;
                            client.Dispose();
                            return true;
                        }
                    }
                    else {
                        loggedUser = null;
                        client.Dispose();
                        return false;
                    }
                }
            }
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


        public static string ConvertToUnsecureString (SecureString securePassword) {
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
        /// <summary>
        /// https://stackoverflow.com/questions/1570422/convert-string-to-securestring/43084626
        /// </summary>
        /// <param name="originalString"></param>
        /// <returns></returns>
        public static SecureString ConvertToSecureString (string originalString) {
            if (originalString == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in originalString)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }

    }
}
