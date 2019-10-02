using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using UnityEngine;

public class ServerActor : MonoBehaviour
{
    public Vector2 currentMoveDirection;
    public List<KeyCode> activeInputs = new List<KeyCode>();
    public ServerClient Client;
    public EndPoint Endpoint { get; internal set; }
    public uint PlayerID;
    public Vector2 CurrentPos;
    public Vector2 WorldPos;


    public int startingX;
    public int startingY;

    public int newX;
    public int newY;

    public bool newMove = true;


    public int Attack = 1;

    void Start()
    {
        CurrentPos = new Vector2(newX, newY);
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
                    currentMoveDirection = new Vector2(-1, 0);
                    break;
                case KeyCode.D:
                    currentMoveDirection = new Vector2(1, 0);
                    break;
                case KeyCode.W:
                    currentMoveDirection = new Vector2(0, 1);
                    break;
                case KeyCode.S:
                    currentMoveDirection = new Vector2(0, -1);
                    break;
                case KeyCode.Space:
                    currentMoveDirection = Vector2.zero;
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
        if (newMove)
        {
            lock (Server.ChangedCells)
            {
                if (!Server.ChangedCells.ContainsKey(Server.MapGrid.grid[startingX, startingY].GetComponent<Cell>()))
                    Server.ChangedCells.Add(Server.MapGrid.grid[startingX, startingY].GetComponent<Cell>(), 0);
                if (!Server.ChangedCells.ContainsKey(Server.MapGrid.grid[newX, newY].GetComponent<Cell>()))
                    Server.ChangedCells.Add(Server.MapGrid.grid[newX, newY].GetComponent<Cell>(), 0);
            }
            Server.MapGrid.grid[newX, newY].GetComponent<Cell>().OccupyCell(gameObject);
            CurrentPos = new Vector2(newX, newY);
            WorldPos = transform.position;
            startingX = newX;
            startingY = newY;
            newMove = false;
        }

        //Debug.Log("Moving");
        if (currentMoveDirection != Vector2.zero)
        {
            lock (Server.ChangedCells)
            {
                Server.MapGrid.grid[newX, newY].GetComponent<Cell>().UnmarkCell();
                if (!Server.ChangedCells.ContainsKey(Server.MapGrid.grid[newX, newY].GetComponent<Cell>()))
                    Server.ChangedCells.Add(Server.MapGrid.grid[newX, newY].GetComponent<Cell>(), PlayerID);
                if (currentMoveDirection.x + startingX < Server.MapGrid.gridWidth && currentMoveDirection.x + startingX >= 0)
                {
                    newX = startingX + (int)currentMoveDirection.x;
                }

                if (currentMoveDirection.y + startingY < Server.MapGrid.gridHeigth && currentMoveDirection.y + startingY >= 0)
                {
                    newY = startingY + (int)currentMoveDirection.y;
                }
                Server.MapGrid.grid[newX, newY].GetComponent<Cell>().MarkCell();
                if (!Server.ChangedCells.ContainsKey(Server.MapGrid.grid[newX, newY].GetComponent<Cell>()))
                    Server.ChangedCells.Add(Server.MapGrid.grid[newX, newY].GetComponent<Cell>(), PlayerID);
                currentMoveDirection = Vector2.zero;
            }
        }



        //}

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
