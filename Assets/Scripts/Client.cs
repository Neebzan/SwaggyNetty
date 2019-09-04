using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



public class Client {
    TcpClient client;
    byte [ ] bytes = new byte [ 256 ];
    PlayerActor actorComponent;
    GameObject actorObject;
    public List<KeyCode> pressedInputs = new List<KeyCode>();

    public Client (TcpClient _client)
    {
        client = _client;
        SpawnActor();
    }

    void SpawnActor ()
    {
        UnityEngine.Object playerPrefab = Resources.Load("Prefabs/PlayerActor");
        actorObject = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        actorComponent = actorObject.GetComponent<PlayerActor>();
        actorComponent.Client = this;
    }

    public IEnumerator ListenForMessages ()
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);

        while (true)
        {
            if (stream.DataAvailable)
            {
                string msg = reader.ReadLine();
                Vector2 moveDir = Vector2.zero;
                Debug.Log(msg);
                string [ ] msgSplit = msg.Split(':');

                for (int i = 0; i < msgSplit.Length; i++)
                {
                    string input = msgSplit [ i ];
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
                            pressedInputs.Add(inputButton);                        
                        else
                            pressedInputs.Remove(inputButton);                        
                    }
                }
            }
            yield return null;
        }
        //GameObject.Destroy(actorObject);
    }
}
