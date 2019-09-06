using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    uint playerID;
    LocalClient client;
    // Start is called before the first frame update
    void Start()
    {
        this.client = GameObject.Find("ClientObject").GetComponent<LocalClient>();
    }


    // Update is called once per frame
    void Update()
    {
        string inputs = string.Empty;

        //Buttons down
        if (Input.GetKeyDown(KeyCode.W))
        {
            inputs += ((int)KeyCode.W).ToString() + ":";
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            inputs += ((int)KeyCode.S).ToString() + ":";
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            inputs += ((int)KeyCode.A).ToString() + ":";
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            inputs += ((int)KeyCode.D).ToString() + ":";
        }


        //Buttons released
        if (Input.GetKeyUp(KeyCode.W))
        {
            inputs += (-(int)KeyCode.W).ToString() + ":";
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            inputs += (-(int)KeyCode.S).ToString() + ":";
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            inputs += (-(int)KeyCode.A).ToString() + ":";
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            inputs += (-(int)KeyCode.D).ToString() + ":";
        }


        //Send inputs to server if there are any
        if (inputs != string.Empty)
            client.Message(inputs);

    }

    

    


    
}
