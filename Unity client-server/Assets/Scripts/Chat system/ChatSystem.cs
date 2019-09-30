using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChatSystem : MonoBehaviour
{

    public GameObject chatPanel, TextObject;
    private string currentMessage = string.Empty;
    private int maxMessages = 30;
    public InputField chatBox;
    public Color playerMessage, group, fail, all;
    public ChatClient cClient;




    [SerializeField]
    public List<Messages> chatHistory = new List<Messages>();




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

        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // send to all
                try
                {
                    cClient.Message(chatBox.text); //sent to server

                }
                catch (Exception e)
                {
                    SendMessageToChat(e.ToString(), Messages.messageTypeColor.fail);
                }
                chatBox.text = "";

                //send to a person
                

                // send to groupe

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
        if (cClient.Connected)
        {

            try
            {

                cClient.ListenToServer();




            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

    }

    public Color MessageTypeColor(Messages.messageTypeColor messageType)
    {
        Color color = group;

        switch (messageType)
        {
            case Messages.messageTypeColor.playerMessage:
                color = playerMessage;
                break;
            case Messages.messageTypeColor.group:
                color = group;
                break;
            case Messages.messageTypeColor.fail:
                color = fail;
                break;
            case Messages.messageTypeColor.all:
                color = all;
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
        group,
        fail,
        all

    }
}

[SerializeField]
public class ChatMessagesPacket
{
    public Messages[] packetMessages;
}
