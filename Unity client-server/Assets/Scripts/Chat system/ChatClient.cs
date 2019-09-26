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
        // task = Task.Factory.StartNew(ListenToServer, TaskCreationOptions.LongRunning);

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
        TCPHelper.MessageBytes(msg);


        Debug.Log("Sent: " + msg);
    }

    public IEnumerator ListenToServer()
    {
        Debug.Log("ListenToServer Started");
        while (true)
        {

            string packet = TCPHelper.ReadMessage(stream);
            if (packet != "")
            {

                Debug.Log("Read: " + packet);
            }

            yield return null;
        }


    }
}
