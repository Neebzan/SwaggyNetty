using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client {
    TcpClient client;
    byte [ ] bytes = new byte [ 256 ];

    public Client (TcpClient _client)
    {
        client = _client;
    }

    public void ListenForMessages ()
    {
        while (true)
        {
            NetworkStream stream = client.GetStream();
            int i;
            string data = null;

            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received: {0}", data);
                data = data.ToUpper();
                byte [ ] msg = System.Text.Encoding.ASCII.GetBytes(data);
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", data);

                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    Log(w, Encoding.ASCII.GetString(bytes, 0, bytes.Length), client.ToString());
                }

            }
        }
    }

    public static void Log (TextWriter log, string msg, string sender)
    {
        Debug.Log("Afsender: " + sender);
        Debug.Log("Tid: " + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString());
        Debug.Log("Besked: " + msg);
        Debug.Log("-----------------------------------------");
        Debug.Log("Logged");

    }
}
