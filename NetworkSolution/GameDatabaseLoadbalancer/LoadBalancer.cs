using GlobalVariablesLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpHelper;

namespace GameDatabaseLoadbalancer
{

    public class LoadBalancer
    {
        TcpListener listener;
        Dictionary<string, int> activeUsers = new Dictionary<string, int>();

        public LoadBalancer()
        {
            listener = new TcpListener(IPAddress.Any, GlobalVariables.GAME_DATABASE_LOADBALANCER_PORT);
            listener.Start();

        }

        void ListenForConnections()
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
            }
        }

        void HandleServerRequests(TcpClient client)
        {
            while (MessageFormatter.Connected(client))
            {
                if (client.GetStream().DataAvailable)
                {
                    List<PlayerDataModel> playerModels;
                    string modelsString = MessageFormatter.ReadStreamOnce(client.GetStream());
                    
                    playerModels = JsonConvert.DeserializeObject<List<PlayerDataModel>>(modelsString);




                }
            }
        }

        void HandlePlayerDataModel(PlayerDataModel model)
        {

        }
    }
}
