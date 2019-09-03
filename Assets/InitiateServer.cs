using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitiateServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Server.StartListening();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
