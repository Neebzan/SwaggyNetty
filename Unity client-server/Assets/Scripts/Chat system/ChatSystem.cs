using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChatSystem : MonoBehaviour
{
    
    public GameObject chatPanel, TextObject;
    private string currentMessage = string.Empty;
    private int maxMessages = 25;
    public InputField chatBox;
    public Color playerMessage, info, fail;
    public ChatClient cClient;


[SerializeField]
    private List<Messages> chatHistory = new List<Messages>();




    public void SendMessageToChat(string text, Messages.messageTypeColor mstype)
    {
        Messages newMessage = new Messages();
        newMessage.text = text;
        if (chatHistory.Count > maxMessages)
        {
            Destroy(chatHistory[0].textObject.gameObject);
            chatHistory.Remove(chatHistory[0]);
        }

        GameObject newText = Instantiate(TextObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text; 
        newMessage.textObject.color = MessageTypeColor(mstype);
        chatHistory.Add(newMessage);

         

    }

    public void Update()
    {
        if(chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat(chatBox.text, Messages.messageTypeColor.playerMessage);
                try
                {
                    cClient.SendMessage(chatBox.text, Messages.messageTypeColor.info); //sent to server
                }
                catch(Exception e)
                {

                    SendMessageToChat(e.ToString(), Messages.messageTypeColor.fail);
                }
                chatBox.text = "";
            }
        }
        else
        {
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();            
            }
        }
        // network info from server
        try
        {
            cClient.ListenToServer();
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }


        //if (!chatBox.isFocused)
        //{
            
            
        //    if (Input.GetKeyDown(KeyCode.Return))
        //    {
        //        SendMessageToChat("W yo", Messages.messageTypeColor.info);
        //    }
        //}      
    }

   public Color MessageTypeColor(Messages.messageTypeColor messageType)
    {
        Color color = info;

        switch (messageType)
        {
            case Messages.messageTypeColor.playerMessage:
                color = playerMessage;
                break;
            case Messages.messageTypeColor.info:
                color = info;
                break;
            default:
                break;
        }

        return color;
    }
}

[System.Serializable]
public class Messages
{
    public string text;
    public Text textObject;
    public messageTypeColor msType;
    public enum messageTypeColor
    {
        playerMessage,
        info,
        fail

      
    }
}

[SerializeField]
public class ChatMessagesPacket
{
    public Messages[] packetMessages;
}
