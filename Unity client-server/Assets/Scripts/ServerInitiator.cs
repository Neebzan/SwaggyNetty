using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ServerInitiator : MonoBehaviour
{
    public static GridGenerater MapGrid;
    void Start()
    {
        StartCoroutine(WaitForClients());
        Task.Factory.StartNew(Server.ListenForClients, TaskCreationOptions.LongRunning);
        MapGrid = GameObject.Find("GameMap").GetComponent<GridGenerater>();
        Server.MapGrid = MapGrid;
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
            while (Server.tcpClients.Count > 0)
            {
                if (Server.tcpClients.TryDequeue(out TcpClient tcpClient))
                {
                    
                    ServerClient client = new ServerClient(tcpClient);
                    //ServerActor actor = client.SpawnActor(index);                    
                    StartCoroutine(client.ListenForMessages());
                }
            }
            yield return null;
        }
    }


}
