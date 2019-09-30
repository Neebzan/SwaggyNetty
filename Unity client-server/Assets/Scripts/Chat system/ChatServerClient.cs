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
    public event Action<List<KeyCode>> OnNewInputsRecieved;
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
                    ChatServer.tickMessages.ChatDataPackages.Add(new ChatData() { SenderName = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString(), Message = msg, port = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port.ToString() });
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

    private void ClientConnected(uint playerID)
    {
        //PositionDataPackage package = new PositionDataPackage()
        //{
        //    PlayerID = playerID,

        //};
        //MessageType msgType = MessageType.Connect;
        //string jsonPackage = JsonUtility.ToJson(package);
        //string msg = ((int)msgType).ToString();
        //msg += ChatServer.MESSAGE_TYPE_INDICATOR + jsonPackage;
        //byte[] byteData = System.Text.Encoding.ASCII.GetBytes(msg);

        //byte[] totalPackage = ChatServer.AddSizeHeaderToPackage(byteData);

        //SendToClient(totalPackage);
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


    //skal nok sende til grupper
    void HandleInputMessage(string msg)
    {


        string[] msgSplit = msg.Split('*');



        for (int i = 0; i < msgSplit.Length; i++)
        {
            string input = msgSplit[i];

            if (input.Contains("*"))
            {

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
        //StreamWriter writer = new StreamWriter(networkStream);

        //TCPHelper.MessageBytes(data);
        networkStream.Write(data, 0, data.Length);

    }
}
