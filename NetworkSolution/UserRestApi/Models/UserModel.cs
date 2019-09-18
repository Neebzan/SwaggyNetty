using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserRestApi.Models
{
    public class UserModel
    {
        [PrimaryKey]
        public string UserID { get; set; }
        public string PswdHash { get; set; }
    }
}