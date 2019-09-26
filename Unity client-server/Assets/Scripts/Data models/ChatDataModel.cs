using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChatData
{
    public string SenderName;
    public string Message;
}

[Serializable]
public class ChatDataPackage
{
    public List<ChatData> ChatDataPackages = new List<ChatData>();
}
