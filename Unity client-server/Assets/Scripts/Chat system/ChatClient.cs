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
        int port = 13000;
        client = new TcpClient("178.155.161.248", port);
        client.NoDelay = true;
        Debug.Log("Connected?");

        stream = client.GetStream();
        StartCoroutine(ListenToServer());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Message(string msg)
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

                string[] tempMsg = msg.Split(Server.MESSAGE_TYPE_INDICATOR);
                MessageType msgType = (MessageType)Int32.Parse(tempMsg[0]);

                switch (msgType)
                {
                    case MessageType.Input:
                        break;
                    case MessageType.Disconnect:
                        break;
                    case MessageType.Connect:
                        {
                            PositionDataPackage temp = JsonUtility.FromJson<PositionDataPackage>(tempMsg[1]);
                            LocalActor t = GameObject.FindGameObjectWithTag("Player").GetComponent<LocalActor>();
                            t.playerID = temp.PlayerID;
                            t.gameObject.transform.position = temp.Position;
                            actors.Add(t);
                            //gameObject.transform.position = temp.Position;
                            connected = true;
                            break;
                        }
                    case MessageType.ServerTick:
                        {

                            ChatMessagesPacket data = JsonUtility.FromJson<ChatMessagesPacket>(tempMsg[1]);

                            for (int i = 0; i < data.packetMessages.Length; i++)
                            {
                                
                            }
                            //PositionDataCollectionPackage data = JsonUtility.FromJson<PositionDataCollectionPackage>(tempMsg[1]);
                            //for (int i = 0; i < data.PositionDataPackages.Length; i++)
                            //{
                            //    bool playerFound = false;
                            //    for (int j = 0; j < actors.Count; j++)
                            //    {
                            //        if (actors[j].playerID == data.PositionDataPackages[i].PlayerID)
                            //        {
                            //            actors[j].gameObject.transform.position = data.PositionDataPackages[i].Position;
                            //            playerFound = true;
                            //        }
                            //    }
                                //if (!playerFound)
                                //{
                                //    GameObject actorObject = (GameObject)GameObject.Instantiate(playerPrefab, data.PositionDataPackages[i].Position, Quaternion.identity);
                                //    LocalActor temp = actorObject.GetComponent<LocalActor>();
                                //    temp.playerID = data.PositionDataPackages[i].PlayerID;
                                //    actors.Add(temp);
                                //}
                            //}
                            break;
                        }
                    default:
                        break;
                }


                //gameObject.transform.position = data.PositionDataPackages[playerID].Position;


            }

            packagesRead++;
            Debug.Log("Read: " + packagesRead + " packages");

            yield return null;
        }
    }
}
