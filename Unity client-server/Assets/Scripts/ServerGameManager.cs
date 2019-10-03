using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class ServerGameManager : MonoBehaviour
{
    public GridGenerater MapGrid;
    public List<ServerActor> Players = new List<ServerActor>();

    public EventHandler TickResolved = delegate { };

    // Start is called before the first frame update
    void Start()
    {
        MapGrid = GameObject.Find("GameMap").GetComponent<GridGenerater>();
    }

    public void ResolveTick(object sender, ElapsedEventArgs e)
    {
        //for (int i = 0; i < Players.Count; i++)
        //{
        //    Players[i].Move();
        //}

        TickResolved.Invoke(this, new EventArgs());
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].Move();
        }
    }
}
