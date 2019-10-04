﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using GlobalVariablesLib;
using JWTlib;

public class ChatClient : MonoBehaviour
{



    TcpClient client;
    ChatSystem chatSystem;
    public string userName = string.Empty;
    public List<ChatGroup> mygroups = new List<ChatGroup>();
    public string chatTarget = "all";
    public string clientName;


  


    // Start is called before the first frame update
    void Start()
    {
        //string[] args = Environment.GetCommandLineArgs();
        //token = args[1];
        
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJKV1RQYXlsb2FkIjoie1wiU2VydmVyc0luZm9cIjp7XCJTZXJ2ZXJzXCI6W119LFwiVXNlcklEXCI6XCJKZW5zXCJ9IiwibmJmIjoxNTcwMTE5MjkwLCJleHAiOjE1NzA1NTEyOTAsImlhdCI6MTU3MDExOTI5MH0.L31Fkm8kaOpVoglhgEv_GvCAD6b1ep0h56OstUnF0d4";

        JwtSecurityToken tokenSent = new JwtSecurityToken(token);
        JWTPayload payload = JWTManager.GetModelFromToken<JWTPayload>(token);
        clientName = payload.UserID;

        chatSystem = gameObject.GetComponent<ChatSystem>();

        int port = 13001;
        string IpAdress = "127.0.0.1";
        client = new TcpClient(IpAdress, port);
        userName = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

        //client.NoDelay = true;
        Debug.Log("Connected?");

     
        StartCoroutine(ListenToServer());
       

    }

    public void AddToMyGroups()
    {

    }

    public string[] MyStringSplitter(string msg)
    {
        return msg.Split().Where(m => m != string.Empty).Distinct().ToArray();
    }

    public bool GetTargetString(string mesg)
    {
        if (mesg[0] == '/')
        {

            try
            {

                mesg = mesg.Remove(0, 1);
                string[] target = MyStringSplitter(mesg);

                if (target[0] == "create")
                {
                    chatTarget = target[0] + " " + target[1];
               
                }
                else if (target[0] == "tell")
                {
                    chatTarget = target[0] + " " + target[1];
                }
                else
                {

                    chatTarget = target[0];
                }
                return true;
            }
            catch
            {
                chatSystem.SendMessageToChat("Invalid command!", Messages.messageTypeColor.fail);
                return false;
            }
        }

        return true;
    }

    public string MessageCleaner(string msg)
    {
        if (msg[0] == '/')
        {
            msg = msg.Remove(0, msg.IndexOf(' '));

        }
            return msg;
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
        //  chatTarget = GetTargetString(msg);
        if (GetTargetString(msg))
        {
            msg = MessageCleaner(msg);
            ChatDataPackage cdp = new ChatDataPackage();
            cdp.ChatDataPackages.Add(new ChatData
            {
                //  SenderName = "Tais",
                SenderName = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString(),
                Message = msg,
                port = ((IPEndPoint)client.Client.LocalEndPoint).Port.ToString(),
                Target = chatTarget,
                PlayerName = clientName
            });



            byte[] data = TCPHelper.MessageBytes(cdp);

            client.GetStream().Write(data, 0, data.Length);

            Debug.Log("Sent: " + msg);
        }
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

