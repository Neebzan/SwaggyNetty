using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



public class Client {
    private TcpClient tcpClient;
    public event Action<List<KeyCode>> OnNewInputsRecieved;
    public List<KeyCode> ActiveInputs = new List<KeyCode>();
    NetworkStream networkStream;

    public Client (TcpClient _tcpClient) {
        tcpClient = _tcpClient;
        SpawnActor();
        networkStream = tcpClient.GetStream();
    }

    /// <summary>
    /// Instantiates a player actor in the scene
    /// </summary>
    void SpawnActor () {
        UnityEngine.Object playerPrefab = Resources.Load("Prefabs/PlayerActor");
        GameObject actorObject = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        PlayerActor actorComponent = actorObject.GetComponent<PlayerActor>();
        actorComponent.Endpoint = tcpClient.Client.RemoteEndPoint;
        this.OnNewInputsRecieved += actorComponent.NewInputsRecieved;
        actorComponent.client = this;
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
                Vector2 moveDir = Vector2.zero;
                Debug.Log(msg);
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
            yield return null;
        }
    }

    /// <summary>
    /// Handles the messages recieves from the client, and converts theses to unity inputs
    /// </summary>
    /// <param name="msg"></param>
    private void HandleMessage (string[] msg) {
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

    public void SendToClient (byte[] data) {
        StreamWriter writer = new StreamWriter(networkStream);

        networkStream.Write(data, 0, data.Length);
    }
}
