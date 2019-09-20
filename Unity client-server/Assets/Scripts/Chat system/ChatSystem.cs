using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ChatSystem : MonoBehaviour
{
    
    public GameObject chatPanel, TextObject;
    private string currentMessage = string.Empty;
    private int maxMessages = 25;
    public InputField chatBox;
    public Color playerMessage, info;


[SerializeField]
    private List<Messages> chatHistory = new List<Messages>();




    public void SendMessageToChat(string text, Messages.messageType mstype)
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
                SendMessageToChat(chatBox.text, Messages.messageType.playerMessage);
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

        if (!chatBox.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat("W yo", Messages.messageType.info);
            }
        }      
    }

   public Color MessageTypeColor(Messages.messageType messageType)
    {
        Color color = info;

        switch (messageType)
        {
            case Messages.messageType.playerMessage:
                color = playerMessage;
                break;
            case Messages.messageType.info:
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
    public messageType msType;
    public enum messageType
    {
        playerMessage,
        info
    }
}
