using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class HeadlessServerInitiator : MonoBehaviour
{
    void Start () {
        StartCoroutine(WaitForClients());
        Task.Factory.StartNew(HeadlessServer.ListenForClients, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// An endless coroutine which handles when new clients are connected,
    /// and instantiates internal clients on unity threads.
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForClients () {
        while (true) {
            while (HeadlessServer.tcpClients.Count > 0) {
                if (HeadlessServer.tcpClients.TryDequeue(out TcpClient tcpClient)) {
                    HeadlessServerClient client = new HeadlessServerClient(tcpClient);
                    StartCoroutine(client.ListenForMessages());
                }
            }
            yield return null;
        }
    }
}
