using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chat : MonoBehaviour
{
    public List<string> chatHistorie = new List<string>();

    private string currentMessage = string.Empty;


    private void OnGUI()
    {
        GUILayout.BeginHorizontal(GUILayout.Width(250));
        currentMessage = GUILayout.TextField(currentMessage);

        if (GUILayout.Button("Send"))
        {
            if (!string.IsNullOrEmpty(currentMessage.Trim()))
            {
                //send over network
                ChatMessage(currentMessage);
                currentMessage = string.Empty;
            }
        }

        if (GUILayout.Button("Clear"))
        {
            ClearChat();
        }


        GUILayout.EndHorizontal();

        foreach(string c in chatHistorie)
        {
            GUILayout.Label(c);
        }
    }

    public void ChatMessage(string message)
    {
        chatHistorie.Add(message);
    }

    public void ClearChat()
    {
        chatHistorie.Clear();
    }
}
