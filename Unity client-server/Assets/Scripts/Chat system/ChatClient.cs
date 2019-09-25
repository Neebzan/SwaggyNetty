using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ChatClient : MonoBehaviour
{

    NetworkStream stream;
    TcpClient client;
    bool connected = false;
    uint playerID;
    List<LocalActor> actors = new List<LocalActor>();

    // Start is called before the first frame update
    void Start()
    {
        int port = 13001;
        string IpAdress = "127.0.0.1";
        client = new TcpClient(IpAdress, port);
        client.NoDelay = true;
        Debug.Log("Connected?");

        stream = client.GetStream();
        StartCoroutine(ListenToServer());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Message(string msg, Messages.messageTypeColor mstype)
    {
        msg += "*" +  mstype;
        msg += "\n";
        // Translate the passed message into ASCII and store it as a Byte array.
        byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);

        // Send the message to the connected TcpServer. 
        stream.Write(data, 0, data.Length);

        Debug.Log("Sent: " + msg);
    }

    public IEnumerator ListenToServer()
    {
        Debug.Log("ListenToServer Started");
        //StreamReader reader = new StreamReader(stream);

        byte[] readBuffer = new byte[4];
        while (true)
        {
            int packagesRead = 0;
            while (stream.DataAvailable && packagesRead < 8)
            {
                //Debug.Log("Data received!");

                int bytesRead = 0;

                while (bytesRead < 4)
                {
                    bytesRead += stream.Read(readBuffer, bytesRead, 4 - bytesRead);
                }

                //Debug.Log("4 Bytes received");

                bytesRead = 0;
                byte[] buffer = new byte[BitConverter.ToInt32(readBuffer, 0)];

                while (bytesRead < buffer.Length)
                {
                    bytesRead += stream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                }
                string msg = System.Text.Encoding.UTF8.GetString(buffer);

                string[] tempMsg = msg.Split(ChatServer.MESSAGE_TYPE_INDICATOR);
                MessageType msgType = (MessageType)Int32.Parse(tempMsg[0]);
                ChatMessageType msgChat = (ChatMessageType)Int32.Parse(tempMsg[0]);

                string[] chMsg = msg.Split('*');

                for (int i = 0; i < chMsg.Length; i++)
                {
                    string input = chMsg[i];

                    if (input.Contains("*"))
                    {
                        //do something
                        //så man har enum om hvem sende som gruppe
                    }


                }

                packagesRead++;
                Debug.Log("Read: " + packagesRead + " packages");

                yield return null;
            }
        }
    }
}
