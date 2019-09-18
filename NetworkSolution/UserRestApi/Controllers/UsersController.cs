using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using UserRestApi.Models;

namespace UserRestApi.Controllers
{
    public class UsersController : ApiController
    {
        string path = "C:/Users/esben/Desktop/TestUserModelDB.db";


        public List<UserModel> Get () {
            List<UserModel> users = new List<UserModel>();
            using (SQLiteConnection connection = new SQLiteConnection(path)) {
                connection.CreateTable<UserModel>();
                TableQuery<UserModel> data = connection.Table<UserModel>();

                foreach (var user in data) {
                    users.Add(user);
                }
            }
            return users;
        }

        public IHttpActionResult Get (string id) {
            UserModel foundUser = null;
            using (SQLiteConnection connection = new SQLiteConnection(path)) {
                connection.CreateTable<UserModel>();
                TableQuery<UserModel> data = connection.Table<UserModel>();
                foundUser = (from UserModel in data
                             where UserModel.UserID == id
                             select UserModel).FirstOrDefault();
            }
            if (foundUser != null) {
                return Ok(foundUser);
            }
            else {
                return NotFound();
            }
        }

        public IHttpActionResult Post (UserModel user) {
            using (SQLiteConnection connection = new SQLiteConnection(path)) {
                connection.CreateTable<UserModel>();
                try {
                    using (HMACSHA512 t = new HMACSHA512(Encoding.UTF8.GetBytes(user.PswdHash))) {
                        byte [ ] hash;
                        hash = t.ComputeHash(Encoding.UTF8.GetBytes(user.PswdHash));
                        //Fjern bindestreger
                        user.PswdHash = BitConverter.ToString(hash).Replace("-", "");

                        connection.Insert(user);
                        return Ok();
                    }
                }
                catch (Exception e) {
                    return Conflict();
                }
            }
        }

        public void Put (int id, [FromBody]string value) {
        }

        public void Delete (int id) {
        }

        [Route("api/users/clear/all")]
        [HttpGet]
        public void Clear () {
            List<UserModel> users = new List<UserModel>();
            using (SQLiteConnection connection = new SQLiteConnection(path)) {
                connection.DropTable<UserModel>();
            }
        }
    }
}
