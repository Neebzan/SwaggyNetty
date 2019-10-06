using GlobalVariablesLib;
using JWTlib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ServerClient
{
    public TcpClient tcpClient;
    public event Action<List<KeyCode>> OnNewInputsRecieved;
    public List<KeyCode> ActiveInputs = new List<KeyCode>();
    NetworkStream networkStream;
    bool isDisconnecting = false;
    public string clientName;

    public ServerClient(TcpClient _tcpClient)
    {
        tcpClient = _tcpClient;
        //ServerActor actor = SpawnActor();
        networkStream = tcpClient.GetStream();
        tcpClient.NoDelay = true;

    }

    /// <summary>
    /// Instantiates a player actor in the scene
    /// </summary>
    public ServerActor SpawnActor(Vector2 index, string name)
    {
        UnityEngine.Object playerPrefab = Resources.Load("Prefabs/ServerActor");

        GameObject actorObject = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        ServerActor actorComponent = actorObject.GetComponent<ServerActor>();
        actorComponent.newX = (int)index.x;
        actorComponent.newY = (int)index.y;
        actorObject.GetComponentInChildren<Text>().text = name;

        actorComponent.Endpoint = tcpClient.Client.RemoteEndPoint;
        this.OnNewInputsRecieved += actorComponent.NewInputsRecieved;
        actorComponent.Client = this;
        Server.PlayersConnected++;
        actorComponent.PlayerID = Server.PlayersConnected;

        ClientConnected(actorComponent.PlayerID, actorComponent.CurrentPos);
        Server.Clients.Add(this);
        Server.Players.Add(actorComponent);
        return actorComponent;
    }

    /// <summary>
    /// Starts an endless while-loop, where the tcp client listens for new messages from the endpoint
    /// </summary>
    /// <returns></returns>
    public IEnumerator ListenForMessages()
    {
        StreamReader reader = new StreamReader(networkStream);

        while (!isDisconnecting)
        {
            if (Connected)
            {
                if (networkStream.DataAvailable)
                {
                    string msg = reader.ReadLine();
                    string[] msgSplit = msg.Split(Server.MESSAGE_TYPE_INDICATOR);
                    MessageType type = (MessageType)Int32.Parse(msgSplit[0]);
                    string newMessage = msgSplit[1];

                    switch (type)
                    {
                        case MessageType.Validate:
                            if (ValidateToken(newMessage))
                            {
                                //For sjov random placering
                                JwtSecurityToken token = new JwtSecurityToken(newMessage);
                                JWTPayload payload = JWTManager.GetModelFromToken<JWTPayload>(token);
                                clientName = payload.UserID;

                                //Tjek om spilleren allerede er ingame
                                foreach (var item in Server.Clients)
                                {
                                    if (item.clientName == clientName)
                                    {
                                        isDisconnecting = true;
                                        DisconnectClient();
                                    }
                                }

                                //Tjek om brugeren har en karakter i databasen
                                PlayerDataModel character = GetCharacterFromDB();

                                if (character != null)
                                {
                                    switch (character.PlayerDataStatus)
                                    {
                                        case PlayerDataStatus.Success:
                                            {
                                                if (!isDisconnecting)
                                                {
                                                    Vector2 index = new Vector2(character.PositionX, character.PositionY);
                                                    Server.MapGrid.grid[(int)index.x, (int)index.y].GetComponent<Cell>().OccupyCell(SpawnActor(index, clientName).gameObject);
                                                }
                                            }
                                            break;
                                        case PlayerDataStatus.DoesNotExist:
                                            {
                                                if (!isDisconnecting)
                                                {
                                                    Vector2 index = new Vector2(UnityEngine.Random.Range(0, Server.MapGrid.gridWidth), UnityEngine.Random.Range(0, Server.MapGrid.gridHeigth));
                                                    Server.MapGrid.grid[(int)index.x, (int)index.y].GetComponent<Cell>().OccupyCell(SpawnActor(index, clientName).gameObject);
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    isDisconnecting = true;
                                    DisconnectClient();
                                }

                            }
                            else
                            {
                                isDisconnecting = true;
                                DisconnectClient();
                            }
                            break;
                        case MessageType.Input:
                            HandleInputMessage(newMessage);
                            break;
                        case MessageType.Disconnect:
                            DisconnectClient();
                            break;
                        case MessageType.Connect:
                            break;
                        case MessageType.ServerTick:
                            break;
                        default:
                            break;
                    }
                }
                yield return null;
            }
            else
            {
                isDisconnecting = true;
                DisconnectClient();
            }
        }
    }

    private PlayerDataModel GetCharacterFromDB()
    {
        TcpClient client = new TcpClient(GlobalVariables.MYSQL_PLAYER_DB_IP, GlobalVariables.GAME_DATABASE_LOADBALANCER_PORT);

        List<PlayerDataModel> requests = new List<PlayerDataModel>();
        PlayerDataModel model = new PlayerDataModel()
        {
            PlayerDataRequest = PlayerDataRequest.Read,
            UserID = clientName
        };
        requests.Add(model);
        //Sent to database
        byte[] data = TCPHelper.MessageBytes(requests);
        Server.databaseClient.GetStream().Write(data, 0, data.Length);

        PlayerDataModel responseModel = null;
        //Wait for response
        while (responseModel == null)
        {
            if (client.GetStream().DataAvailable)
            {
                string responseString = TCPHelper.ReadStreamOnce(client.GetStream());
                PlayerDataModel response = JsonConvert.DeserializeObject<PlayerDataModel>(responseString);
            }
        }
        //When response have been received, check the response
        switch (responseModel.PlayerDataStatus)
        {
            case PlayerDataStatus.None:
                break;
            case PlayerDataStatus.Success:
                return responseModel;
            case PlayerDataStatus.ConnectionFailed:
                break;
            case PlayerDataStatus.AlreadyExists:
                break;
            case PlayerDataStatus.DoesNotExist:
                {
                    if (RequestCharacterCreation(client))
                    {
                        return new PlayerDataModel() { PlayerDataStatus = PlayerDataStatus.DoesNotExist };
                    }
                }
                break;
            default:
                break;
        }
        return null;
    }

    private bool RequestCharacterCreation(TcpClient client)
    {
        List<PlayerDataModel> requests = new List<PlayerDataModel>();
        PlayerDataModel model = new PlayerDataModel()
        {
            PlayerDataRequest = PlayerDataRequest.Create,
            UserID = clientName
        };
        requests.Add(model);
        //Sent to database
        byte[] data = TCPHelper.MessageBytes(requests);
        Server.databaseClient.GetStream().Write(data, 0, data.Length);

        PlayerDataModel responseModel = null;
        //Wait for response
        while (responseModel == null)
        {
            if (client.GetStream().DataAvailable)
            {
                string responseString = TCPHelper.ReadStreamOnce(client.GetStream());
                PlayerDataModel response = JsonConvert.DeserializeObject<PlayerDataModel>(responseString);
            }
        }

        switch (responseModel.PlayerDataStatus)
        {
            case PlayerDataStatus.None:
                break;
            case PlayerDataStatus.Success:
                return true;
            case PlayerDataStatus.ConnectionFailed:
                break;
            case PlayerDataStatus.AlreadyExists:
                break;
            case PlayerDataStatus.DoesNotExist:
                break;
            default:
                break;
        }

        return false;
    }

    private void DisconnectClient()
    {
        isDisconnecting = true;
        tcpClient.Close();
        Server.Disconnect(this);
    }

    public bool ValidateToken(string token)
    {
        byte[] tokenData = TCPHelper.MessageBytes(token);
        TcpClient client = new TcpClient(Globals.MIDDLEWARE_IP, Globals.TOKENSYSTEM_PORT);
        //TcpClient client = new TcpClient("192.168.87.107", GlobalVariables.TOKENSYSTEM_PORT);

        client.GetStream().Write(tokenData, 0, tokenData.Length);
        //Await response from TokenSystem
        bool done = false;
        while (!done)
        {
            if (client.GetStream().DataAvailable)
            {
                Debug.Log("incoming response!");
                string response = TCPHelper.ReadStreamOnce(client.GetStream());

                HttpStatusCode responseCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), response);

                if (responseCode == HttpStatusCode.OK)
                {
                    Debug.Log("VALID");
                    done = true;
                    return true;
                }
                else
                {
                    Debug.Log("INVALID");
                    done = true;
                    return false;
                }
            }
        }

        return false;
    }

    private void ClientConnected(uint playerID, Vector2 playerPos)
    {

        DataCollectionPackage package = new DataCollectionPackage();
        PositionDataPackage pData = new PositionDataPackage()
        {
            PlayerName = clientName,
            PlayerID = playerID,
            Position = playerPos
        };
        package.PositionDataPackages.Add(pData);

        //Første package er mappens dimensioner
        GridDataPackage gData = new GridDataPackage()
        {
            X = Server.MapGrid.gridWidth,
            Y = Server.MapGrid.gridHeigth
        };
        package.GridDataPackages.Add(gData);


        MessageType msgType = MessageType.Connect;
        string jsonPackage = JsonUtility.ToJson(package);
        string msg = ((int)msgType).ToString();
        msg += Server.MESSAGE_TYPE_INDICATOR + jsonPackage;
        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(msg);

        byte[] totalPackage = Server.AddSizeHeaderToPackage(byteData);

        SendToClient(totalPackage);

        try
        {
            UserSession ses = new UserSession() { UserID = clientName, InGame = true, Request = SessionRequest.SetStatus };
            byte[] data = TCPHelper.MessageBytes(ses);
            Server.SessionClient.GetStream().Write(data, 0, data.Length);
        }
        catch
        {

        }
    }

    public bool Connected
    {
        get
        {
            try
            {
                if (tcpClient.Client != null && tcpClient.Client.Connected)
                {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];

                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }

                        return true;
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    void HandleInputMessage(string msg)
    {
        //Vector2 moveDir = Vector2.zero;
        string[] msgSplit = msg.Split(':');

        for (int i = 0; i < msgSplit.Length; i++)
        {
            string input = msgSplit[i];
            KeyCode inputButton;
            bool pressed = true;

            if (input.Contains("-"))
            {
                pressed = false;
                input = input.Remove(0, 1);
            }

            if (Enum.TryParse<KeyCode>(input, out inputButton))
            {
                if (pressed)
                    ActiveInputs.Add(inputButton);
                else
                    ActiveInputs.Remove(inputButton);
                OnNewInputsRecieved.Invoke(ActiveInputs);
            }
        }
    }

    /// <summary>
    /// Handles the messages recieves from the client, and converts theses to unity inputs
    /// </summary>
    /// <param name="msg"></param>
    private void ConvertToInput(string[] msg)
    {
        for (int i = 0; i < msg.Length; i++)
        {
            string input = msg[i];
            KeyCode inputButton;
            bool pressed = true;

            if (input.Contains("-"))
            {
                pressed = false;
                input = input.Remove(0, 1);
            }

            if (Enum.TryParse<KeyCode>(input, out inputButton))
            {
                if (pressed)
                    ActiveInputs.Add(inputButton);
                else
                    ActiveInputs.Remove(inputButton);
                OnNewInputsRecieved.Invoke(ActiveInputs);
            }
        }
    }

    public void SendToClient(byte[] data)
    {
        StreamWriter writer = new StreamWriter(networkStream);
        networkStream.Write(data, 0, data.Length);
    }
}
