using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : MonoBehaviour
{
    // Start is called before the first frame update

    public Client Client;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Move (Vector2 dir)
    {
        transform.Translate(dir);
    }
}
