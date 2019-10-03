using GlobalVariablesLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpHelper;

namespace SessionManager
{
    public class SessionManager
    {
        List<UserSession> sessions = new List<UserSession>();
        TcpListener userListener;
        TcpListener serverListener;


        public SessionManager()
        {
            userListener = new TcpListener(IPAddress.Any, GlobalVariables.SESSION_USER_PORT);
            userListener.Start();
            serverListener = new TcpListener(IPAddress.Any, GlobalVariables.SESSION_SERVER_PORT);
            serverListener.Start();

            Thread t = new Thread(() => ListenForUsers());
            t.IsBackground = true;
            t.Start();

            Thread y = new Thread(() => ListenForServers());
            y.IsBackground = true;
            y.Start();

            Thread u = new Thread(() => PrintUserSessions());
            u.IsBackground = true;
            u.Start();
        }

        void PrintUserSessions()
        {
            while (true)
            {
                lock (sessions)
                    foreach (var item in sessions)
                    {
                        Console.WriteLine("Session: User {0}, ingame status: {1}", item.UserID, item.InGame);
                    }

                Thread.Sleep(2000);
            }

        }

        void HandleServerRequests(TcpClient client)
        {
            while (MessageFormatter.Connected(client))
            {
                if (client.GetStream().DataAvailable)
                {
                    string sessionJson = MessageFormatter.ReadStreamOnce(client.GetStream());

                    UserSession ses = JsonConvert.DeserializeObject<UserSession>(sessionJson);

                    switch (ses.Request)
                    {
                        case SessionRequest.GetAllSessions:
                            {
                                byte[] data = MessageFormatter.MessageBytes(sessions);
                                client.GetStream().Write(data, 0, data.Length);
                            }
                            break;
                        case SessionRequest.GetUserSession:
                            break;
                        case SessionRequest.SetStatus:
                            {
                                foreach (var item in sessions)
                                {
                                    if (item.UserID == ses.UserID)
                                    {
                                        item.InGame = ses.InGame;
                                        Console.WriteLine("User {0} ingame status is now {1}", item.UserID, item.InGame);
                                    }
                                    break;
                                }
                            }
                            break;
                        case SessionRequest.GetOnlineSessions:
                            {
                                List<UserSession> temp = sessions.Where(o => o.InGame == true).ToList();
                                byte[] data = MessageFormatter.MessageBytes(temp);
                                client.GetStream().Write(data, 0, data.Length);
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        void ListenForServers()
        {
            Console.WriteLine("Listening for servers");
            while (true)
            {
                TcpClient client = serverListener.AcceptTcpClient();

                Console.WriteLine("Server Connected!");

                Thread t = new Thread(() => HandleServerRequests(client));
                t.IsBackground = true;
                t.Start();
            }

        }

        void ListenForUsers()
        {
            Console.WriteLine("Listening for users");
            while (true)
            {
                TcpClient client = userListener.AcceptTcpClient();

                Console.WriteLine("User Connected!");
                Thread t = new Thread(() => HandleSession(client));
                t.IsBackground = true;
                t.Start();
            }
        }

        void HandleSession(TcpClient client)
        {
            //Await a UserSession
            bool sessionReceived = false;
            UserSession session = null;
            while (!sessionReceived)
            {
                if (client.GetStream().DataAvailable)
                {
                    string sessionString = MessageFormatter.ReadStreamOnce(client.GetStream());
                    session = JsonConvert.DeserializeObject<UserSession>(sessionString);
                    lock (sessions)
                        sessions.Add(session);
                    sessionReceived = true;
                }
            }
            //If there's an active session, check if there's still a live connection
            while (MessageFormatter.Connected(client))
            {
                Thread.Sleep(1000);
            }
            //If connection was lost, remove session
            lock (sessions)
                sessions.Remove(session);

            Console.WriteLine("Session aborted");
        }
    }
}
