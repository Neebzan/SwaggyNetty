using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Concurrent;

public class ChatServer : MonoBehaviour
{

    // public const char MESSAGE_TYPE_INDICATOR = '?';
    public const int SERVER_PORT = 13001;
    public static ConcurrentQueue<TcpClient> tcpClients = new ConcurrentQueue<TcpClient>();
    public static List<ChatServerClient> Clients = new List<ChatServerClient>();
    static TcpListener listener = new TcpListener(IPAddress.Any, SERVER_PORT);
    public static int idNumber = 0;


    //chat gruops
    public static List<ChatGroup> groups = new List<ChatGroup>();

    public Chat chatGUI;

    public static ChatDataPackage tickMessages = new ChatDataPackage();

    public static uint PlayersConnected;
    // Start is called before the first frame update
    void Start()
    {

    }


    void Update()
    {
        //for (int i = 0; i < Clients.Count; i++)
        //{
        //    chatGUI.chatHistorie.Add(Clients[i].clientName);
        //}



        lock (tickMessages)
        {
            foreach (var item in tickMessages.ChatDataPackages)
            {
                string[] chatTarget = item.Target.Split(' ');

                if (chatTarget[0] == "create")
                {

                    CreateGroup(chatTarget[1], item.SenderClient);
                }
                if (chatTarget[0] == "all")
                {
                    SendToAll();
                }
                else if (chatTarget[0] == "tell")
                {
                    SendWhisper(chatTarget[1], item.SenderClient);
                }
                else
                {
                    for (int i = 0; i < groups.Count; i++)
                    {
                        if (groups[i].GroupName == item.Target)
                        {
                            JoinGroup(groups[i].GroupName, item.SenderClient);
                            SendToGoupe(item.Target);
                        }
                    }
                }

            }
            tickMessages.ChatDataPackages.Clear();
        }

    }

    //create group
    public void CreateGroup(string groupName, ChatServerClient client)
    {
        ChatGroup mygroup = new ChatGroup() { GroupName = groupName, ID = ChatServer.idNumber };
        ChatServer.idNumber += 1;
        bool exists = false;

        foreach (var item in groups)
        {
            if (item.GroupName == groupName)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            ChatServer.groups.Add(mygroup);
        }



        //client.SendMessage("Group create: " + groupName);
        //DEBUG
        if (chatGUI != null)
            foreach (var item in tickMessages.ChatDataPackages)
            {
                chatGUI.chatHistorie.Add(groupName + " created");

            }
        JoinGroup(groupName, client);


    }
    //join group
    public void JoinGroup(string groupName, ChatServerClient client)
    {
        for (int i = 0; i < ChatServer.groups.Count; i++)
        {
            if (ChatServer.groups[i].GroupName == groupName)
            {
                if (!ChatServer.groups[i].Members.Contains(client))
                {

                    ChatServer.groups[i].Members.Add(client);
                }
            }
        }
    }
    // leave group
    public void LeaveGroup(string groupName, ChatServerClient client)
    {
        for (int i = 0; i < ChatServer.groups.Count; i++)
        {
            if (ChatServer.groups[i].GroupName == groupName)
            {
                ChatServer.groups[i].Members.Remove(client);
                var dasGroup = ChatServer.groups[i];
                var mes = TCPHelper.MessageBytes(ChatServer.groups[i]);
                for (int y = 0; y < dasGroup.Members.Count; y++)
                {
                    dasGroup.Members[i].SendToClient(mes);
                }
            }
        }

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
                    chatGUI.chatHistorie.Add(item.PlayerName + " : " + item.port + " : " + item.Message);

                }
            // DEBUG end

            byte[] mes = TCPHelper.MessageBytes(tickMessages);
            for (int i = 0; i < Clients.Count; i++)
            {

                Clients[i].SendToClient(mes);
                chatGUI.chatHistorie.Add(Clients[i].clientName);
            }

        }
    }

    public void SendWhisper(string whisperName, ChatServerClient client)
    {

        if (tickMessages.ChatDataPackages.Count > 0)
        {

            //DEBUG
            if (chatGUI != null)
                foreach (var item in tickMessages.ChatDataPackages)
                {
                    chatGUI.chatHistorie.Add(item.PlayerName + " : " + item.port + " : " + item.Message + " : to: " + item.Target);

                }


            byte[] mes = TCPHelper.MessageBytes(tickMessages);
            //skal finde playeren man whisper
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i].clientName == whisperName)
                {
                    Clients[i].SendToClient(mes);
                    client.SendToClient(mes);

                }
            }

        }
    }

    public void SendToGoupe(string groupName)
    {

        if (tickMessages.ChatDataPackages.Count > 0)
        {

            //DEBUG
            if (chatGUI != null)
                foreach (var item in tickMessages.ChatDataPackages)
                {
                    chatGUI.chatHistorie.Add(item.Target + " : " + item.port + " : " + item.Message);

                }


            byte[] mes = TCPHelper.MessageBytes(tickMessages);
            //skal finde gruppen

            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].GroupName == groupName)
                {
                    for (int y = 0; y < groups[i].Members.Count; y++)
                    {

                        groups[i].Members[y].SendToClient(mes);

                    }
                }

            }

        }
    }


}
