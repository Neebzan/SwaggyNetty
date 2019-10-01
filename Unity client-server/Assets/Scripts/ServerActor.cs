using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerActor : MonoBehaviour
{
    public Vector2 currentMoveDirection;
    private List<KeyCode> activeInputs = new List<KeyCode>();
    public ServerClient Client;
    public EndPoint Endpoint { get; internal set; }
    public uint PlayerID;
    public Vector2 CurrentPos;
    public Vector2 CurrentGridIndex;
    private GridGenerater map = Server.MapGrid;

    void Start()
    {
        CurrentPos = transform.position;
    }

    void Update()
    {
        SetMoveInput();
        Move();
    }

    /// <summary>
    /// Determines how the given inputs should be interpreted
    /// </summary>
    public void SetMoveInput()
    {
        currentMoveDirection = Vector2.zero;
        foreach (KeyCode input in activeInputs)
        {
            switch (input)
            {
                case KeyCode.A:
                    currentMoveDirection += new Vector2(-1, 0);
                    break;
                case KeyCode.D:
                    currentMoveDirection += new Vector2(1, 0);
                    break;
                case KeyCode.W:
                    currentMoveDirection += new Vector2(0, 1);
                    break;
                case KeyCode.S:
                    currentMoveDirection += new Vector2(0, -1);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Moves the player actor in the direction of the last known inputs
    /// </summary>
    public void Move()
    {

        if (currentMoveDirection.x != 0)
            if (currentMoveDirection.x + CurrentGridIndex.x < map.gridWidth && currentMoveDirection.x + CurrentGridIndex.x > 0)
                //map.grid[CurrentGridIndex+currentMoveDirection.x]

        //currentMoveDirection.Normalize();
        //if (currentMoveDirection != Vector2.zero)
        //    transform.Translate(currentMoveDirection * Time.deltaTime * 4.0f);
        //CurrentPos = transform.position;
    }

    /// <summary>
    /// Raised by the client when new inputs are recieved
    /// </summary>
    /// <param name="newInputs"></param>
    public void NewInputsRecieved(List<KeyCode> newInputs)
    {
        this.activeInputs = newInputs;
    }
}
