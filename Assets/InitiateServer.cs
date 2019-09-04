using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class InitiateServer : MonoBehaviour {

    void Start ()
    {
        StartCoroutine(WaitForClients());
        Task.Factory.StartNew(Server.ListenForClients, TaskCreationOptions.LongRunning);
    }

    public IEnumerator WaitForClients ()
    {
        while (true)
        {
            while (Server.tcpClients.Count > 0)
            {
                if (Server.tcpClients.TryDequeue(out TcpClient tcpClient))
                {
                    Client client = new Client(tcpClient);
                    StartCoroutine(client.ListenForMessages());
                }

            }
            yield return null;
        }
    }
}
