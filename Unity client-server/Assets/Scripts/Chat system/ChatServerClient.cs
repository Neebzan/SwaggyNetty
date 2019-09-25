using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
                TCPHelper.ReadMessage(networkStream);
                
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
    }

    private void ClientConnected(uint playerID)
    {
        PositionDataPackage package = new PositionDataPackage()
        {
            PlayerID = playerID,
           
        };
        MessageType msgType = MessageType.Connect;
        string jsonPackage = JsonUtility.ToJson(package);
        string msg = ((int)msgType).ToString();
        msg += ChatServer.MESSAGE_TYPE_INDICATOR + jsonPackage;
        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(msg);

        byte[] totalPackage = ChatServer.AddSizeHeaderToPackage(byteData);

        SendToClient(totalPackage);
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
           
            if(input.Contains("*"))
            {

            }  

          
        }
    }

    /// <summary>
    /// Handles the messages recieves from the client, and converts theses to unity inputs
    /// </summary>
    /// <param name="msg"></param>
    private void ConvertToInput(string[] msg)
    {
        for (int i = 0; i < msg.Length; i++)
        {
            string input = msg[i];
            KeyCode inputButton;
            bool pressed = true;

            if (input.Contains("-"))
            {
                pressed = false;
                input = input.Remove(0, 1);
            }

            if (Enum.TryParse<KeyCode>(input, out inputButton))
            {
                if (pressed)
                    ActiveInputs.Add(inputButton);
                else
                    ActiveInputs.Remove(inputButton);
                OnNewInputsRecieved.Invoke(ActiveInputs);
            }
        }
    }

    public void SendToClient(byte[] data)
    {
        //StreamWriter writer = new StreamWriter(networkStream);

        //networkStream.Write(data, 0, data.Length);

        TCPHelper.MessageBytes(data);
    }
}
