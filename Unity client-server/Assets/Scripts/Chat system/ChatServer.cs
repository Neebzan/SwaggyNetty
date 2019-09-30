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
    public const int SERVER_PORT = 13001;
    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();
    public static List<ChatServerClient> Clients = new List<ChatServerClient>();
    static TcpListener listener = new TcpListener(IPAddress.Any, SERVER_PORT);
    public string playerName;




    public Chat chatGUI;

    public static ChatDataPackage tickMessages = new ChatDataPackage();

    public static uint PlayersConnected;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    // send en besked i starten som siger om man vil joine en gruppe 
    // hvem den bliver sendt til
    //serveren skal tjekke hvem beskeden er til.

    void Update()
    {
        SendToAll();
    }

    public string GetLocalIPAddress()
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

    public static void Disconnect(ChatServerClient disconnectedClient)
    {
        Clients.Remove(disconnectedClient);
    }

    public void SendToAll()
    {
        if (tickMessages.ChatDataPackages.Count > 0)
        {
            //DEBUG
            if (chatGUI != null)
                foreach (var item in tickMessages.ChatDataPackages)
                {
                    chatGUI.chatHistorie.Add(item.SenderName + " : " + item.port + " : " + item.Message);
                    
                }
            // DEBUG end

            byte[] mes = TCPHelper.MessageBytes(tickMessages);
            for (int i = 0; i < Clients.Count; i++)
            {
                
                Clients[i].SendToClient(mes);
               // chatGUI.chatHistorie.Add(Clients[i].ToString());
            }
            tickMessages.ChatDataPackages.Clear();
        }
    }

    public void SendWhisper()
    {

        if (tickMessages.ChatDataPackages.Count > 0)
        {

            //sende til en ip perosn. skal hente ip på person ud fra et navn clienten sender
            byte[] mes = TCPHelper.MessageBytes(tickMessages);
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i].ToString() == playerName)
                {
                    Clients[i].SendToClient(mes);

                }
            }
            tickMessages.ChatDataPackages.Clear();
        }
    }

    public void SendToGoupe()
    {

        if (tickMessages.ChatDataPackages.Count > 0)
        {
            // skal tjekke igennem en liste at gruppen kan der  problem. hvordan får man fat i en gruppe genneralt når vi ikke ved hvad det er for en liste som skal bruges
            byte[] mes = TCPHelper.MessageBytes(tickMessages);
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].SendToClient(mes);
            }
            tickMessages.ChatDataPackages.Clear();
        }
    }


}
