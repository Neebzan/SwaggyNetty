using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : MonoBehaviour {
    // Start is called before the first frame update

    public Client Client;
    Vector2 currentMoveDirection;

    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        SetMoveInput();
        Move();
    }

    internal void SetMoveInput ()
    {
        currentMoveDirection = Vector2.zero;
        foreach (KeyCode input in Client.pressedInputs)
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

    private void Move ()
    {
        currentMoveDirection.Normalize();
        if (currentMoveDirection != Vector2.zero)
        {
            transform.Translate(currentMoveDirection * Time.deltaTime * 1.0f);
        }

    }
}
