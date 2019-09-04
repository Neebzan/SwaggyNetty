using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class Server {
    const int SERVER_PORT = 13000;
    static IPAddress iPAd = IPAddress.Parse("10.131.68.191");
    static TcpListener listener = new TcpListener(iPAd, SERVER_PORT);

    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();

    public static void ListenForClients ()
    {
        listener.Start();
        Debug.Log("Started listening");
        while (true)
        {
            Debug.Log("Waiting for connection");
            TcpClient c = listener.AcceptTcpClient();
            Debug.Log("Client connected");
            tcpClients.Enqueue(c);
            Debug.Log(c.Client.RemoteEndPoint.ToString() + " connected");
        }
    }
}
