using GlobalVariablesLib;
using GlobalVariablesLib.Models;
using Launcher.Properties;
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
        public static UserModel loggedUser { get; private set; } = null;
        public static float PatchProgress = 0f;
        public static float ConnectionTimeoutMS = 5000.0f;
        public static FileTransferModel PatchData = null;
        public static string GamePath = @"Downloads";
        public static string GameName = @"\SwaggyNetty.exe";

        /// <summary>
        /// Invoke this when an error is encountered in the backend. Simply call the DisplayError method directly if encountered in the application instead.
        /// </summary>
        public static EventHandler<BackendErrorEventArgs> BackendErrorEncountered;

        static Backend () {
            PatchmanagerClient.MissingFilesUpdated += PatchDataUpdated;
        }

        /// <summary>
        /// Raised whenever new data has been recieved from the patch manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void PatchDataUpdated (object sender, EventArgs e) {
            if (PatchmanagerClient.MissingFiles.RemainingSize == 0) {
                PatchProgress = 100;
            }
            else
                PatchProgress = ((1.0f - ((float)PatchmanagerClient.MissingFiles.RemainingSize / (float)PatchmanagerClient.MissingFiles.TotalSize)) * 100.0f);
        }

        /// <summary>
        /// Instantiates a task to run the patchmanager client functionality
        /// </summary>
        public static void InitiatePatchClient () {
            Task t = new Task(() => PatchmanagerClient.StartPatchCheck(@"Downloads"));
            t.Start();
            PatchData = PatchmanagerClient.MissingFiles;
        }

        /// <summary>
        /// Establishes a connection to the middleware service
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static TcpClient ConnectoToMiddleware (float timeout) {
            TcpClient client = new TcpClient();

            try {
                var result = client.BeginConnect(GlobalVariables.MIDDLEWARE_IP, GlobalVariables.MIDDLEWARE_PORT, null, null);
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

        /// <summary>
        /// Writes, via. an established client connection, to the middleware service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static async Task<bool> WriteToMiddleware (TcpClient client, UserModel user) {
            byte [ ] msg = TcpHelper.MessageFormatter.MessageBytes<UserModel>(user);

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

        /// <summary>
        /// Sends a login request to the middleware service
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<bool> SendLoginCredentials (string username, SecureString password) {
            string unsecurePassword = ConvertToUnsecureString(password);
            string hashedPassword = GetHashedString(unsecurePassword);

            TcpClient client = ConnectoToMiddleware(ConnectionTimeoutMS);

            if (client == null)
                return false;

            UserModel user = new UserModel() { UserID = username, PswdHash = hashedPassword, RequestType = GlobalVariablesLib.RequestType.Get_User };



            if (!await WriteToMiddleware(client, user))
                return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed.TotalMilliseconds <= ConnectionTimeoutMS) {
                string result = TcpHelper.MessageFormatter.ReadStreamOnce(client.GetStream());
                if (!string.IsNullOrEmpty(result)) {
                    UserModel resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(result);
                    if (resultUser.RequestType != RequestType.Error) {
                        if (resultUser.RequestType == RequestType.Token_Get) {
                            loggedUser = resultUser;
                            client.Dispose();
                            return true;
                        }
                    }
                    else {
                        client.Dispose();
                        BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Login request failed", resultUser.Message));
                        return false;
                    }
                }
            }

            client.Dispose();
            BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Client connection failed", "The client connection timed-out after " + ConnectionTimeoutMS.ToString() + " ms"));
            return false;
        }

        /// <summary>
        /// Sends a login request to the middleware service, with a JWT
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static async Task<bool> SendTokenLogin (string token, string userID) {
            TcpClient client = ConnectoToMiddleware(ConnectionTimeoutMS);

            if (client == null)
                return false;

            UserModel user = new UserModel() { UserID = userID, Token = token, RequestType = RequestType.Token_Check };

            if (!await WriteToMiddleware(client, user))
                return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < ConnectionTimeoutMS) {
                string result = TcpHelper.MessageFormatter.ReadStreamOnce(client.GetStream());
                if (!string.IsNullOrEmpty(result)) {
                    UserModel resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(result);
                    if (resultUser.RequestType != RequestType.Error) {
                        if (resultUser.TokenResponse == TokenResponse.Valid) {
                            loggedUser = resultUser;
                            client.Dispose();
                            return true;
                        }
                        else if (resultUser.TokenResponse == TokenResponse.Invalid) {
                            client.Dispose();
                            BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Token login failed", "Your session has either expired or token was invalid"));
                            return false;
                        }
                    }
                    else {
                        client.Dispose();
                        BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Token login failed", resultUser.Message));
                        return false;
                    }
                }
            }

            client.Dispose();
            BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Client connection failed", "The client connection timed-out after " + ConnectionTimeoutMS.ToString() + " ms"));
            return false;
        }

        /// <summary>
        /// Sends a register request to the middleware service
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<bool> SendRegisterRequest (string username, SecureString password) {
            string unsecurePassword = ConvertToUnsecureString(password);
            string hashedPassword = GetHashedString(unsecurePassword);

            TcpClient client = ConnectoToMiddleware(ConnectionTimeoutMS);

            if (client == null)
                return false;

            UserModel user = new UserModel() { UserID = username, PswdHash = hashedPassword, RequestType = RequestType.Create_User };

            if (!await WriteToMiddleware(client, user))
                return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < ConnectionTimeoutMS) {
                string result = TcpHelper.MessageFormatter.ReadStreamOnce(client.GetStream());
                if (!string.IsNullOrEmpty(result)) {
                    UserModel resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(result);
                    if (resultUser.RequestType != RequestType.Error) {
                        if (resultUser.RequestType == RequestType.Create_User) {
                            loggedUser = resultUser;
                            client.Dispose();
                            return true;
                        }
                    }
                    else {
                        loggedUser = null;
                        client.Dispose();
                        BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Create user failed", resultUser.Message));
                        return false;
                    }
                }
            }

            client.Dispose();
            BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Client connection failed", "The client connection timed-out after " + ConnectionTimeoutMS.ToString() + " ms"));
            return false;
        }

        /// <summary>
        /// Logs the user out of the backend code by removing the run-time instance of the model
        /// </summary>
        public static void Logout () {
            loggedUser = null;
        }

        /// <summary>
        /// !! NOTE IMPLEMENTED YET !! Launches the game client, with the token as paramter
        /// </summary>
        public static void LaunchGame () {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(GamePath + GameName, Settings.Default.SessionToken);
            if (!p.Start())
                BackendErrorEncountered?.Invoke(null, new BackendErrorEventArgs("Launch game error", "Could not launch the game"));
            else
                Environment.Exit(0);
        }

        /// <summary>
        /// Returns a HMACSHA512 version of a string 
        /// </summary>
        /// <param name="_input"></param>
        /// <returns></returns>
        private static string GetHashedString (string _input) {

            using (HMACSHA512 t = new HMACSHA512(Encoding.UTF8.GetBytes(_input))) {
                byte [ ] hash;
                hash = t.ComputeHash(Encoding.UTF8.GetBytes(_input));
                _input = BitConverter.ToString(hash).Replace("-", "");
            }

            return _input;
        }

        /// <summary>
        /// Checks if two password SecureStrings are the same value
        /// </summary>
        /// <param name="_password"></param>
        /// <param name="_confirmPass"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a SecureString password to a normal readable string
        /// </summary>
        /// <param name="securePassword"></param>
        /// <returns></returns>
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
