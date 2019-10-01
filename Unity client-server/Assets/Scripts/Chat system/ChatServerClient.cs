using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ChatServerClient : MonoBehaviour
{
    private TcpClient tcpClient;
    public List<KeyCode> ActiveInputs = new List<KeyCode>();
    NetworkStream networkStream;
    bool isDisconnecting = false;



    public ChatServerClient(TcpClient _tcpClient)
    {
        tcpClient = _tcpClient;

        networkStream = tcpClient.GetStream();
        tcpClient.NoDelay = true;

        ChatServer.Clients.Add(this);

    }

    // skal modtage besked. og sende den videre.


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
                    string msg = TCPHelper.ReadMessage(networkStream);
                    ChatServer.tickMessages.ChatDataPackages.Add(new ChatData() { SenderName = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString(),
                        Message = msg, port = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port.ToString() });
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
