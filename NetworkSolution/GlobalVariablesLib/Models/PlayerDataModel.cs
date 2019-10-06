using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib.Models {

    public enum PlayerDataRequest { Create, Read, Update, Delete};
    public enum PlayerDataStatus { Success, Failure};

   public class PlayerDataModel {
        public string UserID { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public bool Online { get; set; }
        public PlayerDataRequest PlayerDataRequest { get; set; }
        public PlayerDataStatus  PlayerDataStatus { get; set; }
    }
}
