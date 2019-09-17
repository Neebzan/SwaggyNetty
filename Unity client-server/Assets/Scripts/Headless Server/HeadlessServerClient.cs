using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class HeadlessServerClient {
    private TcpClient tcpClient;
    public event Action<List<KeyCode>> OnNewInputsRecieved;
    public List<KeyCode> ActiveInputs = new List<KeyCode>();
    NetworkStream networkStream;
    bool isDisconnecting = false;

    public HeadlessServerClient (TcpClient _tcpClient) {
        tcpClient = _tcpClient;
        HeadlessServerActor actor = SpawnActor();
        networkStream = tcpClient.GetStream();
        tcpClient.NoDelay = true;
        ClientConnected(actor.PlayerID, actor.CurrentPos);
        HeadlessServer.Clients.Add(this);
        HeadlessServer.Players.Add(actor);
    }

    /// <summary>
    /// Instantiates a player actor in the scene
    /// </summary>
    HeadlessServerActor SpawnActor () {
        HeadlessServerActor actor = new HeadlessServerActor();
        actor.Endpoint = tcpClient.Client.RemoteEndPoint;
        this.OnNewInputsRecieved += actor.NewInputsRecieved;
        actor.Client = this;
        HeadlessServer.PlayersConnected++;
        actor.PlayerID = HeadlessServer.PlayersConnected;
        return actor;
    }

    /// <summary>
    /// Starts an endless while-loop, where the tcp client listens for new messages from the endpoint
    /// </summary>
    /// <returns></returns>
    public IEnumerator ListenForMessages () {
        StreamReader reader = new StreamReader(networkStream);

        while (!isDisconnecting) {
            if (Connected) {
                if (networkStream.DataAvailable) {
                    string msg = reader.ReadLine();
                    string [ ] msgSplit = msg.Split(HeadlessServer.MESSAGE_TYPE_INDICATOR);
                    MessageType type = (MessageType)Int32.Parse(msgSplit [ 0 ]);
                    string newMessage = msgSplit [ 1 ];

                    switch (type) {
                        case MessageType.Input:
                            HandleInputMessage(newMessage);
                            break;
                        case MessageType.Disconnect:
                            DisconnectClient();
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
            else {
                isDisconnecting = true;
                DisconnectClient();
            }
        }
    }

    private void DisconnectClient () {
        isDisconnecting = true;
        Console.WriteLine(tcpClient.Client.RemoteEndPoint.ToString() + " disconnected");
        tcpClient.Close();
        HeadlessServer.Disconnect(this);
    }

    private void ClientConnected (uint playerID, Vector2 playerPos) {
        PositionDataPackage package = new PositionDataPackage() {
            PlayerID = playerID,
            Position = playerPos
        };
        MessageType msgType = MessageType.Connect;
        string jsonPackage = JsonUtility.ToJson(package);
        string msg = ((int)msgType).ToString();
        msg += HeadlessServer.MESSAGE_TYPE_INDICATOR + jsonPackage;
        byte [ ] byteData = System.Text.Encoding.ASCII.GetBytes(msg);

        byte [ ] totalPackage = HeadlessServer.AddSizeHeaderToPackage(byteData);

        SendToClient(totalPackage);
        
    }

    public bool Connected {
        get {
            try {
                if (tcpClient.Client != null && tcpClient.Client.Connected) {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead)) {
                        byte [ ] buff = new byte [ 1 ];

                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0) {
                            return false;
                        }

                        return true;
                    }

                    return true;
                }

                return false;
            }
            catch {
                return false;
            }
        }
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
