﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ChatClient : MonoBehaviour
{


    TcpClient client;
    // Task task;
    ChatSystem chatSystem;
    public string userName = string.Empty; 
  



    // Start is called before the first frame update
    void Start()
    {
        chatSystem = gameObject.GetComponent<ChatSystem>();

        int port = 13001;
        string IpAdress = "127.0.0.1";
        client = new TcpClient(IpAdress, port);
        userName = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

        //client.NoDelay = true;
        Debug.Log("Connected?");

        //stream = client.GetStream();
        StartCoroutine(ListenToServer());
        //task = Task.Factory.StartNew(ListenToServer, TaskCreationOptions.LongRunning);

    }

    public bool Connected
    {
        get
        {
            try
            {
                if (client.Client != null && client.Client.Connected)
                {
                    if (client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];

                        if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }

                        return true;
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public void Message(string msg)
    {
        byte[] data = TCPHelper.MessageBytes(msg);
        client.GetStream().Write(data, 0, data.Length);

        Debug.Log("Sent: " + msg);
    }

    public IEnumerator ListenToServer()
    {
        Debug.Log("ListenToServer Started");

        while (true)
        {
            if (client.GetStream().DataAvailable)
            {
                string packetString = TCPHelper.ReadMessage(client.GetStream());
                ChatDataPackage packet = JsonUtility.FromJson<ChatDataPackage>(packetString);
                foreach (var item in packet.ChatDataPackages)
                {
                    // dele op i grupper her
                  

                    chatSystem.SendMessageToChat(item.Message, Messages.messageTypeColor.all);
                    //Debug.Log("Read: " + packet);
                }
            }
            yield return null;
        }


    }

}
