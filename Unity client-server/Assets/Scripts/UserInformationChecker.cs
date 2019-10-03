using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class UserInformationChecker : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        ValidateUser();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ValidateUser()
    {
        string[] args = Environment.GetCommandLineArgs();
        string token = args[1];

        GameObject.Find("TokenText").GetComponent<Text>().text = token;

        //byte[] tokenData = TCPHelper.MessageBytes(token);
        //TcpClient client = new TcpClient(GlobalVariables.MIDDLEWARE_IP, GlobalVariables.TOKENSYSTEM_PORT);
        //client.GetStream().Write(tokenData, 0, tokenData.Length);




        
        //string userName = "";
        //string address = "";
        //int port = -1;
        

        //foreach (string arg in args)
        //{
        //    if (arg.Contains("Username="))
        //    {
        //        userName = arg.Split('=')[1];
        //    }
        //    if (arg.Contains("IPAddress="))
        //    {
        //        address = arg.Split('=')[1];
        //    }
        //    if (arg.Contains("Port="))
        //    {
        //        int.TryParse(arg.Split('=')[1], out port);
        //    }
        //}
    }
}
