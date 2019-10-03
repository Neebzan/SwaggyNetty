using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ChatServerClient 
{
    private TcpClient tcpClient;
    public List<KeyCode> ActiveInputs = new List<KeyCode>();
    NetworkStream networkStream;
    bool isDisconnecting = false;
    public string clientName;


    public ChatServerClient(TcpClient _tcpClient)
    {
        tcpClient = _tcpClient;

        networkStream = tcpClient.GetStream();
        tcpClient.NoDelay = true;
        clientName = tcpClient.Client.RemoteEndPoint.ToString();
       // ChatServer.Clients.Add(this);

    }


    /// <summary>
    /// Starts an endless while-loop, where the tcp client listens for new messages from the endpoint
    /// </summary>
    /// <returns></returns>
    public IEnumerator ListenForMessages()
    {
        while (!isDisconnecting)
        {
            if (Connected)
            {
                if (networkStream.DataAvailable)
                {
                    ChatDataPackage msg = JsonUtility.FromJson<ChatDataPackage>(TCPHelper.ReadMessage(networkStream));
                    foreach (var item in msg.ChatDataPackages)
                    {
                        item.SenderClient = this;
                        lock (ChatServer.tickMessages)
                        {
                            ChatServer.tickMessages.ChatDataPackages.Add(item);
                        }

                    }
                    Debug.Log(msg);
                }
                yield return null;
            }
            else
            {
                isDisconnecting = true;
                DisconnectClient();
            }
        }
    }

    private void DisconnectClient()
    {
        isDisconnecting = true;
        tcpClient.Close();
        ChatServer.Disconnect(this);
        Debug.Log("Client disconnected");
    }

    public bool Connected
    {
        get
        {
            try
            {
                if (tcpClient.Client != null && tcpClient.Client.Connected)
                {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];

                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
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


    public string ReceiveFromClient()
    {
        string packetICarry = TCPHelper.ReadMessage(networkStream);
        return packetICarry;
    }

    public void SendToClient(byte[] data)
    {
        networkStream.Write(data, 0, data.Length);

    }
}
