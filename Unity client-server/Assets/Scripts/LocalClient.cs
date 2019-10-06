using GlobalVariablesLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalClient : MonoBehaviour
{
    NetworkStream stream;
    TcpClient client;
    bool connected = false;
    uint playerID;
    List<LocalActor> actors = new List<LocalActor>();
    public UnityEngine.Object playerPrefab;
    GridGenerater map;
    string token;

    // Start is called before the first frame update
    void Start()
    {

        //string[] args = Environment.GetCommandLineArgs();
        //token = args[1];
        //GameObject.Find("TokenText").GetComponent<Text>().text = token;
        token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJKV1RQYXlsb2FkIjoie1wiU2VydmVyc0luZm9cIjp7XCJTZXJ2ZXJzXCI6W119LFwiVXNlcklEXCI6XCJKZW5zXCJ9IiwibmJmIjoxNTcwMTE5MjkwLCJleHAiOjE1NzA1NTEyOTAsImlhdCI6MTU3MDExOTI5MH0.L31Fkm8kaOpVoglhgEv_GvCAD6b1ep0h56OstUnF0d4";

        int port = 13000;

        client = new TcpClient(Globals.MIDDLEWARE_IP, port);
        //client = new TcpClient("192.168.10.135", port);
       


        client.NoDelay = true;
        Debug.Log("Connected?");
        stream = client.GetStream();
        SendTokenToServer();
        StartCoroutine(ListenToServer());

        //playerPrefab = Resources.Load("Prefabs/LocalActor");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SendTokenToServer()
    {
        byte[] tokenData = TCPHelper.MessageBytes(token);
        TcpClient client = new TcpClient(Globals.MIDDLEWARE_IP, Globals.TOKENSYSTEM_PORT);
        client.GetStream().Write(tokenData, 0, tokenData.Length);
        string validateRequest = ((int)MessageType.Validate).ToString() + Server.MESSAGE_TYPE_INDICATOR + token;
        Message(validateRequest);

    }

    public void OnMapLoad()
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
            while (stream.DataAvailable && packagesRead < 2)
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
                        {
                            Debug.Log("Disconnect detected");
                            uint discPlayerID = UInt32.Parse(tempMsg[1]);
                            GameObject disconnectedPlayer = null;
                            foreach (var item in actors)
                            {
                                if (item.playerID == discPlayerID)
                                {
                                    disconnectedPlayer = item.gameObject;
                                    break;
                                }
                            }
                            if (disconnectedPlayer != null)
                                GameObject.Destroy(disconnectedPlayer);
                        }
                        break;
                    case MessageType.Connect:
                        {
                            //SceneManager.LoadScene("Client");
                            //OnMapLoad();
                            DataCollectionPackage data = JsonUtility.FromJson<DataCollectionPackage>(tempMsg[1]);
                            //PositionDataPackage temp = JsonUtility.FromJson<PositionDataPackage>(tempMsg[1]);
                            map = GameObject.Find("GameMap").GetComponent<GridGenerater>();

                            map.Generate(data.GridDataPackages[0].X, data.GridDataPackages[0].Y);

                            LocalActor t = GameObject.FindGameObjectWithTag("Player").GetComponent<LocalActor>();
                            t.gameObject.GetComponentInChildren<Text>().text = data.PositionDataPackages[0].PlayerName;
                            t.playerID = data.PositionDataPackages[0].PlayerID;
                            t.gameObject.transform.position = data.PositionDataPackages[0].Position;
                            actors.Add(t);
                            //gameObject.transform.position = temp.Position;
                            connected = true;
                            break;
                        }
                    case MessageType.ServerTick:
                        {
                            DataCollectionPackage data = JsonUtility.FromJson<DataCollectionPackage>(tempMsg[1]);
                            for (int i = 0; i < data.PositionDataPackages.Count; i++)
                            {
                                bool playerFound = false;
                                for (int j = 0; j < actors.Count; j++)
                                {
                                    if (actors[j].playerID == data.PositionDataPackages[i].PlayerID)
                                    {
                                        actors[j].gameObject.transform.position = data.PositionDataPackages[i].Position;
                                        playerFound = true;
                                    }
                                }
                                foreach (var item in data.GridDataPackages)
                                {
                                    map.grid[item.X, item.Y].GetComponent<Cell>().SetColor(item.Color);
                                    //Debug.Log($"Set Color of tile:{item.X},{item.Y} to {item.Color}");
                                }
                                if (!playerFound)
                                {
                                    GameObject actorObject = (GameObject)GameObject.Instantiate(playerPrefab, data.PositionDataPackages[i].Position, Quaternion.identity);
                                    actorObject.GetComponentInChildren<Text>().text = data.PositionDataPackages[i].PlayerName;
                                    LocalActor temp = actorObject.GetComponent<LocalActor>();
                                    temp.playerID = data.PositionDataPackages[i].PlayerID;
                                    actors.Add(temp);
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }


                //gameObject.transform.position = data.PositionDataPackages[playerID].Position;
            }

            packagesRead++;
            //Debug.Log("Read: " + packagesRead + " packages");
            yield return null;
        }

    }


    private void OnDestroy()
    {
        //Message(((int)MessageType.Disconnect).ToString() + Server.MESSAGE_TYPE_INDICATOR);


        stream.Close();
        client.Close();
    }
}


