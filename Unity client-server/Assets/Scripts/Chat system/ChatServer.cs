using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Concurrent;

public class ChatServer : MonoBehaviour
{

    public const char MESSAGE_TYPE_INDICATOR = '?';
    public const int SERVER_PORT = 13000;
    public static IPAddress iPAd = IPAddress.Parse("10.131.69.85");
    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();
    public static List<ChatServerClient> Clients = new List<ChatServerClient>();
    static TcpListener listener = new TcpListener(iPAd, SERVER_PORT);

    public static uint PlayersConnected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public  string GetLocalIPAddress()
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

    public  void ListenForClients()
    {
        listener.Start();
        while (true)
        {
            TcpClient c = listener.AcceptTcpClient();
            tcpClients.Enqueue(c);
            Debug.Log(c.Client.RemoteEndPoint.ToString() + " connected");
        }
    }

    public static void Disconnect(ChatServerClient disconnectedClient)
    {
        
            Clients.Remove(disconnectedClient);
        

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

    public void SendString()
    {
      // send to clients
    }
   
 
}
