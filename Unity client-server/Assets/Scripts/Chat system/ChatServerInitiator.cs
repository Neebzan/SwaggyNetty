using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ChatServerInitiator : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitForClients());
        Task.Factory.StartNew(ChatServer.ListenForClients, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// An endless coroutine which handles when new clients are connected,
    /// and instantiates internal clients on unity threads.
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForClients()
    {
        while (true)
        {
            while (ChatServer.tcpClients.Count > 0)
            {
                if (ChatServer.tcpClients.TryDequeue(out TcpClient tcpClient))
                {
                    Debug.Log("YAY CLIENT");
                    ChatServerClient client = new ChatServerClient(tcpClient);
                    StartCoroutine(client.ListenForMessages());
                }
            }
            yield return null;
        }
    }
}
