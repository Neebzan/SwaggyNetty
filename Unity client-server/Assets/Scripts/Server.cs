﻿using GlobalVariablesLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using UnityEngine;



public static class Server
{
    public const char MESSAGE_TYPE_INDICATOR = '?';
    public const int SERVER_PORT = 13000;
    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();
    public static List<ServerActor> Players = new List<ServerActor>();
    public static List<ServerClient> Clients = new List<ServerClient>();

    //static TcpListener listener = new TcpListener(iPAd, SERVER_PORT);
    static TcpListener listener = new TcpListener(IPAddress.Any, SERVER_PORT);
    public static TcpClient SessionClient;
    public static TcpClient databaseClient;


    public static System.Timers.Timer roundTimer = new System.Timers.Timer(1000);
    public static System.Timers.Timer packageTimer = new System.Timers.Timer(16.667);
    public static System.Timers.Timer saveStateTimer = new System.Timers.Timer(1000);


    public static uint PlayersConnected;

    public static GridGenerater MapGrid;
    //public static List<Cell> ChangedCells = new List<Cell>();
    public static Dictionary<Cell, uint> ChangedCells = new Dictionary<Cell, uint>();
    static List<ServerClient> disconnectedClients = new List<ServerClient>();


    static Server()
    {
        //databaseClient = new TcpClient(GlobalVariables.MYSQL_PLAYER_DB_IP, GlobalVariables.GAME_DATABASE_LOADBALANCER_PORT);
        databaseClient = new TcpClient(GlobalVariables.LOADBALANCER_IP, GlobalVariables.GAME_DATABASE_LOADBALANCER_PORT);
        StartTick();
        UnityEngine.Application.quitting += StopServer;
        try
        {
            SessionClient = new TcpClient("127.0.0.1", GlobalVariables.SESSION_SERVER_PORT);
        }
        catch
        {
            Debug.Log("No connection to sessionmanager!");
        }
    }


    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    /// <summary>
    /// Starts an endless loop, where the tcpListener continuesly looks for new clients
    /// </summary>
    public static void ListenForClients()
    {
        listener.Start();
        while (true)
        {
            TcpClient c = listener.AcceptTcpClient();
            tcpClients.Enqueue(c);
            Debug.Log(c.Client.RemoteEndPoint.ToString() + " connected");
        }
    }

    private static void StartTick()
    {
        roundTimer.Elapsed += RoundTick;
        roundTimer.AutoReset = true;
        roundTimer.Enabled = true;
        packageTimer.Elapsed += PackageTick;
        packageTimer.AutoReset = true;
        packageTimer.Enabled = true;
        saveStateTimer.Elapsed += SaveTick;
        saveStateTimer.AutoReset = true;
        saveStateTimer.Enabled = true;

    }

    private static void SaveTick(object sender, ElapsedEventArgs e)
    {
        if (Players.Count > 0)
        {
            List<PlayerDataModel> characterSaves = new List<PlayerDataModel>();
            lock (Players)
                foreach (var item in Players)
                {
                    characterSaves.Add(new PlayerDataModel()
                    {
                        PlayerDataRequest = PlayerDataRequest.Update,
                        Online = true,
                        PositionX = item.startingX,
                        PositionY = item.startingY,
                        UserID = item.Client.clientName,
                        ResponseExpected = false
                    });
                }

            byte[] data = TCPHelper.MessageBytesNewton(characterSaves);
            databaseClient.GetStream().Write(data, 0, data.Length);
        }


    }

    private static void PackageTick(object sender, ElapsedEventArgs e)
    {

        lock (Server.ChangedCells)
        {
            List<PositionDataPackage> positionData = PositionData();

            for (int i = 0; i < Players.Count; i++)
            {
                List<GridDataPackage> gridData = GridData(Players[i].PlayerID);

                byte[] package = PackageToByte(new DataCollectionPackage() { PositionDataPackages = positionData, GridDataPackages = gridData });
                if (TCPHelper.Connected(Players[i].Client.tcpClient))
                    Players[i].Client.SendToClient(package);
                else
                    disconnectedClients.Add(Players[i].Client);
            }
            ChangedCells.Clear();
        }

        foreach (var item in disconnectedClients)
        {
            Disconnect(item);
        }
        disconnectedClients.Clear();
    }

    private static void RoundTick(object sender, ElapsedEventArgs e)
    {

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].newMove = true;
        }
    }

    private static void StopServer()
    {
        roundTimer.Enabled = false;
        packageTimer.Enabled = false;
    }

    private static List<GridDataPackage> GridData(uint playerID)
    {
        //Grid data
        List<GridDataPackage> gridPackageCollection = new List<GridDataPackage>();
        foreach (var item in ChangedCells)
        {
            if (item.Value == playerID || item.Value == 0)
                gridPackageCollection.Add(new GridDataPackage() { X = item.Key.X, Y = item.Key.Y, Color = item.Key.color });
        }

        return gridPackageCollection;

        //DataCollectionPackage package = new DataCollectionPackage()
        //{
        //    GridDataPackages = gridPackageCollection
        //};
    }

    private static List<PositionDataPackage> PositionData()
    {
        //Create package from player ID and position
        List<PositionDataPackage> packageCollection = new List<PositionDataPackage>();
        for (int i = 0; i < Players.Count; i++)
        {
            packageCollection.Add(
            new PositionDataPackage
            {
                PlayerName = Players[i].Client.clientName,
                PlayerID = Players[i].PlayerID,
                Position = Players[i].WorldPos
            });
        }

        return packageCollection;

    }

    public static byte[] PackageToByte(DataCollectionPackage package)
    {
        MessageType msgType = MessageType.ServerTick;

        string packageJson = JsonUtility.ToJson(package);
        string msg = ((int)msgType).ToString() + Server.MESSAGE_TYPE_INDICATOR + packageJson;
        //Convert to JSON
        byte[] packageData = System.Text.Encoding.ASCII.GetBytes(msg);

        byte[] totalPackage = AddSizeHeaderToPackage(packageData);

        return totalPackage;
    }

    public static void Disconnect(ServerClient disconnectedClient)
    {
        //Session manager - UNTESTED
        try
        {
            UserSession ses = new UserSession() { UserID = disconnectedClient.clientName, InGame = false, Request = SessionRequest.SetStatus };
            byte[] data = TCPHelper.MessageBytes(ses);
            Server.SessionClient.GetStream().Write(data, 0, data.Length);
        }
        catch
        {

        }

        ServerActor disconnectedActor = null;
        foreach (ServerActor actor in Players)
        {
            if (actor.Client == disconnectedClient)
            {
                disconnectedActor = actor;
                SendDisconnectNotification(disconnectedActor.PlayerID);
            }
        }
        if (disconnectedActor != null)
        {
            //Gem spillerens state
            List<PlayerDataModel> saveStates = new List<PlayerDataModel>();
            saveStates.Add(new PlayerDataModel()
            {
                PlayerDataRequest = PlayerDataRequest.Update,
                Online = false,
                PositionX = disconnectedActor.startingX,
                PositionY = disconnectedActor.startingY,
                UserID = disconnectedActor.Client.clientName,
                ResponseExpected = false
            });
            byte[] data = TCPHelper.MessageBytesNewton(saveStates);
            databaseClient.GetStream().Write(data, 0, data.Length);

            //Fjern fra spillet
            MapGrid.grid[disconnectedActor.startingX, disconnectedActor.startingY].GetComponent<Cell>().UnoccupyCell();
            ChangedCells.Add(Server.MapGrid.grid[disconnectedActor.startingX, disconnectedActor.startingY].GetComponent<Cell>(), 0);
            Players.Remove(disconnectedActor);
            GameObject.Destroy(disconnectedActor.gameObject);
            Clients.Remove(disconnectedClient);
        }

    }

    /// <summary>
    /// Sends a notification to connected clients, that a player has disconnected
    /// </summary>
    /// <param name="playerID"></param>
    private static void SendDisconnectNotification(uint playerID)
    {
        string msg = ((int)MessageType.Disconnect).ToString();
        msg += MESSAGE_TYPE_INDICATOR + playerID.ToString();
        byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
        byte[] totalPackage = AddSizeHeaderToPackage(data);

        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].PlayerID != playerID)
            {

                Players[i].Client.SendToClient(totalPackage);
            }
        }
    }

    public static byte[] AddSizeHeaderToPackage(byte[] package)
    {
        //Create a uint containing the length of the package, and encode to byte array
        int pckLen = package.Length;
        byte[] packageHeader = BitConverter.GetBytes(pckLen);
        byte[] totalPackage = new byte[packageHeader.Length + package.Length];
        //Merge byte arrays
        System.Buffer.BlockCopy(packageHeader, 0, totalPackage, 0, packageHeader.Length);
        System.Buffer.BlockCopy(package, 0, totalPackage, packageHeader.Length, package.Length);

        return totalPackage;
    }
}
