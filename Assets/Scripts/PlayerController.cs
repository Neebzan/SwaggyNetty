using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    NetworkStream stream;
    TcpClient client;

    // Start is called before the first frame update
    void Start()
    {
        int port = 13000;
        client = new TcpClient("10.131.68.191", port);
        client.NoDelay = true;
        Debug.Log("Connected?");
        stream = client.GetStream();
        StartCoroutine(ListenToServer());
    }


    // Update is called once per frame
    void Update()
    {
        string inputs = string.Empty;

        //Buttons down
        if (Input.GetKeyDown(KeyCode.W))
        {
            inputs += ((int)KeyCode.W).ToString() + ":";
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            inputs += ((int)KeyCode.S).ToString() + ":";
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            inputs += ((int)KeyCode.A).ToString() + ":";
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            inputs += ((int)KeyCode.D).ToString() + ":";
        }


        //Buttons released
        if (Input.GetKeyUp(KeyCode.W))
        {
            inputs += (-(int)KeyCode.W).ToString() + ":";
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            inputs += (-(int)KeyCode.S).ToString() + ":";
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            inputs += (-(int)KeyCode.A).ToString() + ":";
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            inputs += (-(int)KeyCode.D).ToString() + ":";
        }


        //Send inputs to server if there are any
        if (inputs != string.Empty)
            Message(inputs);

    }

    void Message(string msg)
    {
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

            if (stream.DataAvailable)
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
                Debug.Log(msg);

            }


            yield return null;
        }


    }


    private void OnDestroy()
    {
        client.Close();
        stream.Close();
    }
}
