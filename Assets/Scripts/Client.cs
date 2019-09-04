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

    void MoveCommand (Vector2 dir)
    {
        actorComponent.Move(dir);
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
                Debug.Log(msg);
                Vector2 moveDir = Vector2.zero;
                if (msg.Contains("w"))
                {
                    moveDir += new Vector2(0, 1);
                }
                if (msg.Contains("a"))
                {
                    moveDir += new Vector2(-1, 0);
                }
                if (msg.Contains("s"))
                {
                    moveDir += new Vector2(0, -1);
                }
                if (msg.Contains("d"))
                {
                    moveDir += new Vector2(1, 0);
                }
                MoveCommand(moveDir);
            }
            yield return null;
        }
        //GameObject.Destroy(actorObject);
    }
}
