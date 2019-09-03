using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;




public static class Server {
    const int SERVER_PORT = 13000;
    static IPAddress iPAd = IPAddress.Parse("10.131.68.123");
    static TcpListener listener = new TcpListener(iPAd, SERVER_PORT);
    static List<Client> clients = new List<Client>();

    public static void StartListening ()
    {
        Console.WriteLine("Started listening");
        Debug.Log("Started listening");

        //Thread t = new Thread(ListenForClients);
        //t.Start();

        Task t = new Task(() => ListenForClients());
        t.Start();
    }

    public static void ListenForClients ()
    {

        listener.Start();
        while (true)
        {
            TcpClient c = listener.AcceptTcpClient();
            Client client = new Client(c);
            clients.Add(client);
            Console.WriteLine("{0} Connected", c.Client.RemoteEndPoint.ToString());
            Debug.Log(c.Client.RemoteEndPoint.ToString() + " Connected!");
            Task t = new Task(() => client.ListenForMessages());
            t.Start();
            Debug.Log("Total clients: " + clients.Count);
        }
    }


}
