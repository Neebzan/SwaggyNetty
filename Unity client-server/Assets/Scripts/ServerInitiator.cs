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
                    //For sjov random placering
                    Vector2 index = new Vector2(Random.Range(0, MapGrid.gridWidth), Random.Range(0, MapGrid.gridHeigth));

                    ServerClient client = new ServerClient(tcpClient);
                    ServerActor actor = client.SpawnActor(index);
                    MapGrid.grid[(int)index.x, (int)index.y].GetComponent<Cell>().OccupyCell(actor.gameObject);
                    StartCoroutine(client.ListenForMessages());
                }
            }
            yield return null;
        }
    }


}
