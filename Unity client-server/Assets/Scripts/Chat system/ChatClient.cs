using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ChatClient : MonoBehaviour
{

    NetworkStream stream;
    TcpClient client;
    Task task;
    ChatSystem chatSystem;
    string groups = "/groups";
    string all = "/all";
    string newString = string.Empty;



    // Start is called before the first frame update
    void Start()
    {
        chatSystem = gameObject.GetComponent<ChatSystem>();

        int port = 13001;
        string IpAdress = "127.0.0.1";
        client = new TcpClient(IpAdress, port);

        //client.NoDelay = true;
        Debug.Log("Connected?");

        //stream = client.GetStream();
        StartCoroutine(ListenToServer());
        //task = Task.Factory.StartNew(ListenToServer, TaskCreationOptions.LongRunning);

    }

    // Update is called once per frame
    void Update()
    {

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

                    if (item.Message.Contains("/group"))
                    {
                       // newString = item.Message.TrimStart(groups);
                      //  item.Message = newString;

                    }
                    if (item.Message.Contains("/all"))
                    {

                    }

                    chatSystem.SendMessageToChat(item.Message, Messages.messageTypeColor.playerMessage);
                    //Debug.Log("Read: " + packet);
                }
            }
            yield return null;
        }


    }

}

