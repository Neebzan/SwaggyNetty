using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class HeadlessServerActor {
    Vector2 currentMoveDirection;
    private List<KeyCode> activeInputs = new List<KeyCode>();
    public HeadlessServerClient Client;
    public EndPoint Endpoint { get; internal set; }
    public uint PlayerID;
    public Vector2 CurrentPos;

    public HeadlessServerActor () {
        CurrentPos = new Vector2(0, 0);
    }

    public void Update (float deltaTime) {
        SetMoveInput();
        Move(deltaTime);
        
    }

    /// <summary>
    /// Determines how the given inputs should be interpreted
    /// </summary>
    public void SetMoveInput () {
        currentMoveDirection = Vector2.zero;
        foreach (KeyCode input in activeInputs) {
            switch (input) {
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
            if (currentMoveDirection!=Vector2.zero) {
                currentMoveDirection.Normalize();
            }
        }
        
    }

    /// <summary>
    /// Moves the player actor in the direction of the last known inputs
    /// </summary>
    private void Move (float deltaTime) {
        if (currentMoveDirection != Vector2.zero) {
            CurrentPos += currentMoveDirection * deltaTime * 4.0f;
        }
    }

    /// <summary>
    /// Raised by the client when new inputs are recieved
    /// </summary>
    /// <param name="newInputs"></param>
    public void NewInputsRecieved (List<KeyCode> newInputs) {
        this.activeInputs = newInputs;
        Console.WriteLine(Endpoint + " new input");
    }
}
