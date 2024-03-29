﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using UnityEngine;



public static class HeadlessServer
    {
    public const char MESSAGE_TYPE_INDICATOR = '?';
    public const int SERVER_PORT = 13000;
    public static IPAddress iPAd = IPAddress.Parse("10.131.69.85");
    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();
    public static List<HeadlessServerActor> Players = new List<HeadlessServerActor>();
    public static List<HeadlessServerClient> Clients = new List<HeadlessServerClient>();

    static TcpListener listener = new TcpListener(iPAd, SERVER_PORT);

    static float deltaTime = 0.0016667f;
    static System.Timers.Timer timer = new System.Timers.Timer(deltaTime * 1000);
    public static uint PlayersConnected;

    static HeadlessServer () {
        StartTick();
        UnityEngine.Application.quitting += StopServer;
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
        while (true) {
            TcpClient c = listener.AcceptTcpClient();
            tcpClients.Enqueue(c);
            Console.WriteLine(c.Client.RemoteEndPoint.ToString() + " connected");
        }
    }

    private static void StartTick () {
        timer.Elapsed += Tick;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private static void Tick (object sender, ElapsedEventArgs e) {
        Update();
        byte [ ] package = PositionData();
        for (int i = 0; i < Players.Count; i++) {
            Players [ i ].Client.SendToClient(package);
        }
    }

    private static void Update () {
        for (int i = 0; i < Players.Count; i++) {
            Players [ i ].Update(deltaTime);
        }
    }

    private static void StopServer () {
        timer.Enabled = false;
    }

    private static byte [ ] PositionData () {
        //Create package from player ID and position
        List<PositionDataPackage> packageCollection = new List<PositionDataPackage>();
        for (int i = 0; i < Players.Count; i++)
        {
            packageCollection.Add(
            new PositionDataPackage
            {
                PlayerID = Players[i].PlayerID,
                Position = Players[i].CurrentPos
            });
        }
        DataCollectionPackage package = new DataCollectionPackage()
        {
            PositionDataPackages = packageCollection
        };

        MessageType msgType = MessageType.ServerTick;

        string packageJson = JsonUtility.ToJson(package);
        string msg = ((int)msgType).ToString() + HeadlessServer.MESSAGE_TYPE_INDICATOR + packageJson;
        //Convert to JSON
        byte [ ] packageData = System.Text.Encoding.ASCII.GetBytes(msg);

        byte [ ] totalPackage = AddSizeHeaderToPackage(packageData);

        return totalPackage;
    }

    public static void Disconnect (HeadlessServerClient disconnectedClient) {
        HeadlessServerActor disconnectedActor = null;
        foreach (HeadlessServerActor actor in Players) {
            if (actor.Client == disconnectedClient) {
                disconnectedActor = actor;
                SendDisconnectNotification(disconnectedActor.PlayerID);
            }
        }
        if (disconnectedActor != null) {
            Players.Remove(disconnectedActor);
            Clients.Remove(disconnectedClient);
        }

    }

    /// <summary>
    /// Sends a notification to connected clients, that a player has disconnected
    /// </summary>
    /// <param name="playerID"></param>
    private static void SendDisconnectNotification (uint playerID) {
        string msg = ((int)MessageType.Disconnect).ToString();
        msg += playerID.ToString();
        byte [ ] data = System.Text.Encoding.ASCII.GetBytes(msg);
        byte [ ] totalPackage = AddSizeHeaderToPackage(data);

        for (int i = 0; i < Players.Count; i++) {
            if (Players[i].PlayerID != playerID) {
                Players [ i ].Client.SendToClient(totalPackage);
            }
        }
    }

    public static byte [ ] AddSizeHeaderToPackage (byte [ ] package) {
        //Create a uint containing the length of the package, and encode to byte array
        int pckLen = package.Length;
        byte [ ] packageHeader = BitConverter.GetBytes(pckLen);
        byte [ ] totalPackage = new byte [ packageHeader.Length + package.Length ];
        //Merge byte arrays
        System.Buffer.BlockCopy(packageHeader, 0, totalPackage, 0, packageHeader.Length);
        System.Buffer.BlockCopy(package, 0, totalPackage, packageHeader.Length, package.Length);

        return totalPackage;
    }
}
