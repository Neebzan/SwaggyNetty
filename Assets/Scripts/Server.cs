using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;

public static class Server {
    public static readonly int SERVER_PORT = 13000;
    public static readonly IPAddress iPAd = IPAddress.Parse("10.131.68.191");
    static TcpListener listener = new TcpListener(iPAd, SERVER_PORT);
    static System.Timers.Timer timer = new System.Timers.Timer(1000);

    static Server () {
        StartTick();
        UnityEngine.Application.quitting += StopServer;
    }



    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();

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
        Debug.Log("Tick");
    }

    private static void StopServer () {
        timer.Enabled = false;
    }
}
