using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib {

    public enum PlayerDataRequest { Create, Read, Update, Delete};
    public enum PlayerDataStatus { None, Success, ConnectionFailed, AlreadyExists, DoesNotExist};

   public class PlayerDataModel {
        public string UserID { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public bool Online { get; set; }
        public PlayerDataRequest PlayerDataRequest { get; set; }
        public PlayerDataStatus  PlayerDataStatus { get; set; }
        public int ReadSlaveNumber { get; set; } = 1;
    }
}
