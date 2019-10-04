using System;
using System.Collections.Generic;
using UnityEngine;
using static Messages;

[Serializable]
public class ChatData
{
    public string SenderName;
    public string port;
    public string Message;
    public string Target;
    public ChatServerClient SenderClient;
    public string PlayerName;
    public messageTypeColor typeColor;

}

[Serializable]
public class ChatDataPackage
{
    
    public List<ChatData> ChatDataPackages = new List<ChatData>();
}
