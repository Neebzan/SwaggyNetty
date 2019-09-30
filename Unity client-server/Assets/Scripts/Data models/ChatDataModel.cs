using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChatData
{
    public string SenderName;
    public string port;
    public string Message;  
    public enum messageTypeColor
    {
        playerMessage,
        group,
        fail,
        all

    }
}

[Serializable]
public class ChatDataPackage
{
    public List<ChatData> ChatDataPackages = new List<ChatData>();
}
