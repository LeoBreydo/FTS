using System;
using System.Collections.Generic;
using MsgBroker.Models.Common;

namespace MsgBroker.Models.AuthorizationScope
{
    public class UserInfo
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public UserInfo()
        {
            Login = "";
            Password = "";
        }

        public UserInfo(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login)) throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
            Login = login;
            Password = password;
        }
    }

    public class AllUsers
    {
        public string AdminLogin=string.Empty, AdminPassword=string.Empty;
        public List<UserInfo> Users = new List<UserInfo>();

        public AllUsers()
        {
            
        }
    }
}
