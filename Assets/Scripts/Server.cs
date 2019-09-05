using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using UnityEngine;



public static class Server {
    public static readonly int SERVER_PORT = 13000;
    public static IPAddress iPAd = IPAddress.Parse("10.131.68.191");
    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();
    public static List<PlayerActor> Players = new List<PlayerActor>();
    public static List<Client> Clients = new List<Client>();

    static TcpListener listener = new TcpListener(iPAd, SERVER_PORT);
    static System.Timers.Timer timer = new System.Timers.Timer(1000);
    public static uint PlayersConnected;

    static Server () {
        StartTick();
        UnityEngine.Application.quitting += StopServer;
        //iPAd = IPAddress.Parse(GetLocalIPAddress());
        //Debug.Log(iPAd.ToString());
    }

    public static string GetLocalIPAddress () {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }


    /// <summary>
    /// Starts an endless loop, where the tcpListener continuesly looks for new clients
    /// </summary>
    public static void ListenForClients () {
        listener.Start();
        Debug.Log("Listening for clients");

        while (true) {
            TcpClient c = listener.AcceptTcpClient();
            tcpClients.Enqueue(c);
            Debug.Log(c.Client.RemoteEndPoint.ToString() + " connected");
        }
    }

    private static void StartTick () {
        timer.Elapsed += Tick;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private static void Tick (object sender, ElapsedEventArgs e) {
        byte[] package = PositionData();
        for (int i = 0; i < Players.Count; i++) {
            Players [ i ].Client.SendToClient(package);
        }
    }

    private static void StopServer () {
        timer.Enabled = false;
    }

    private static byte [ ] PositionData () {
        //Create package from player ID and position
        PositionDataPackage [ ] packageCollection = new PositionDataPackage [ Players.Count ];
        for (int i = 0; i < Players.Count; i++) {
            PositionDataPackage pck = new PositionDataPackage {
                PlayerID = Players [ i ].PlayerID,
                Position = Players [ i ].CurrentPos
            };
            packageCollection [ i ] = pck;
        }
        PositionDataCollectionPackage package = new PositionDataCollectionPackage() {
            PositionDataPackages = packageCollection
        };

        string packageJson = JsonUtility.ToJson(package);

        //Convert to JSON
        byte [ ] packageData = System.Text.Encoding.ASCII.GetBytes(packageJson);

        //Create a uint containing the length of the package, and encode to byte array
        int pckLen = packageData.Length;
        byte [ ] packageHeader = BitConverter.GetBytes(pckLen);
        byte [ ] totalPackage = new byte [ packageHeader.Length + packageData.Length ];
        //Merge byte arrays
        System.Buffer.BlockCopy(packageHeader, 0, totalPackage, 0, packageHeader.Length);
        System.Buffer.BlockCopy(packageData, 0, totalPackage, packageHeader.Length, packageData.Length);

        return totalPackage;
    }
}
