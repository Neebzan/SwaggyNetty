using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



public class ServerClient {
    private TcpClient tcpClient;
    public event Action<List<KeyCode>> OnNewInputsRecieved;
    public List<KeyCode> ActiveInputs = new List<KeyCode>();
    NetworkStream networkStream;

    public ServerClient (TcpClient _tcpClient) {
        tcpClient = _tcpClient;
        ServerActor actor = SpawnActor();
        networkStream = tcpClient.GetStream();
        tcpClient.NoDelay = true;
        ClientConnected(actor.PlayerID, actor.CurrentPos);
        Server.Clients.Add(this);
        Server.Players.Add(actor);
    }

    /// <summary>
    /// Instantiates a player actor in the scene
    /// </summary>
    ServerActor SpawnActor () {
        UnityEngine.Object playerPrefab = Resources.Load("Prefabs/ServerActor");

        GameObject actorObject = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        ServerActor actorComponent = actorObject.GetComponent<ServerActor>();
        actorComponent.Endpoint = tcpClient.Client.RemoteEndPoint;
        this.OnNewInputsRecieved += actorComponent.NewInputsRecieved;
        actorComponent.Client = this;
        Server.PlayersConnected++;
        actorComponent.PlayerID = Server.PlayersConnected;
        return actorComponent;
    }

    /// <summary>
    /// Starts an endless while-loop, where the tcp client listens for new messages from the endpoint
    /// </summary>
    /// <returns></returns>
    public IEnumerator ListenForMessages () {
        StreamReader reader = new StreamReader(networkStream);

        while (true) {
            if (networkStream.DataAvailable) {
                string msg = reader.ReadLine();

                MessageType type = (MessageType)Int32.Parse(msg.Split('?') [ 0 ]);

                switch (type) {
                    case MessageType.Input:
                        HandleInputMessage(msg);
                        break;
                    case MessageType.Disconnect:
                        break;
                    case MessageType.Connect:
                        break;
                    case MessageType.ServerTick:
                        break;
                    default:
                        break;
                }



            }
            yield return null;
        }
    }

    private void ClientConnected (uint playerID, Vector2 playerPos) {
        PositionDataPackage package = new PositionDataPackage() {
            PlayerID = playerID,
            Position = playerPos
        };

        string jsonPackage = JsonUtility.ToJson(package);
        byte [ ] byteData = System.Text.Encoding.ASCII.GetBytes(jsonPackage);

        byte [ ] totalPackage = Server.AddSizeHeaderToPackage(byteData);

        SendToClient(totalPackage);
    }


    void HandleInputMessage (string msg) {
        Vector2 moveDir = Vector2.zero;
        string [ ] msgSplit = msg.Split(':');

        for (int i = 0; i < msgSplit.Length; i++) {
            string input = msgSplit [ i ];
            KeyCode inputButton;
            bool pressed = true;

            if (input.Contains("-")) {
                pressed = false;
                input = input.Remove(0, 1);
            }

            if (Enum.TryParse<KeyCode>(input, out inputButton)) {
                if (pressed)
                    ActiveInputs.Add(inputButton);
                else
                    ActiveInputs.Remove(inputButton);
                OnNewInputsRecieved.Invoke(ActiveInputs);
            }
        }
    }

    /// <summary>
    /// Handles the messages recieves from the client, and converts theses to unity inputs
    /// </summary>
    /// <param name="msg"></param>
    private void ConvertToInput (string [ ] msg) {
        for (int i = 0; i < msg.Length; i++) {
            string input = msg [ i ];
            KeyCode inputButton;
            bool pressed = true;

            if (input.Contains("-")) {
                pressed = false;
                input = input.Remove(0, 1);
            }

            if (Enum.TryParse<KeyCode>(input, out inputButton)) {
                if (pressed)
                    ActiveInputs.Add(inputButton);
                else
                    ActiveInputs.Remove(inputButton);
                OnNewInputsRecieved.Invoke(ActiveInputs);
            }
        }
    }

    public void SendToClient (byte [ ] data) {
        StreamWriter writer = new StreamWriter(networkStream);

        networkStream.Write(data, 0, data.Length);
    }
}
