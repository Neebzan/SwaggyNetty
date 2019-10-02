using System;
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
    public List<ChatGroup> mygroups = new List<ChatGroup>();
    public string chatTarget = "all";



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

    public void AddToMyGroups()
    {

    }
    public string GetTargetString(string mesg)
    {
        if (mesg[0] == '/')
        {
               

            //if (chatTarget.Split(' ')[0] == "create")
            //{
            //    chatTarget = chatTarget.Split(' ')[1];
            //}
           mesg = mesg.Remove(0, 1);
            string[] target = mesg.Split(' ');
            if(target[0] == "create")
            {
                chatTarget =  target[0] + " " + target[1];
            }
            if (target[0] == "tell")
            {
                chatTarget =  target[0] + " " + target[1];
            }
             chatTarget = target[0];
        }
        
        return chatTarget;
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
        chatTarget = GetTargetString(msg);
        ChatDataPackage cdp = new ChatDataPackage();
        cdp.ChatDataPackages.Add(new ChatData
        {
            SenderName = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString(),
            Message = msg,
            port = ((IPEndPoint)client.Client.LocalEndPoint).Port.ToString(),
            Target = chatTarget
        });



        byte[] data = TCPHelper.MessageBytes(cdp);

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
                    string senderClient = item.SenderName;
                    string msg = senderClient + ": " + item.Message;
                    chatSystem.SendMessageToChat(msg, Messages.messageTypeColor.playerMessage);
                    msg = "";
                }
            }
            yield return null;
        }


    }

}

