using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    [Serializable]
    public class ChatGroup
    {

        public int ID { get; set; }
        public string GoupName { get; set; }
    public List<ChatServerClient> members { get; set; } = new List<ChatServerClient>();

    }

