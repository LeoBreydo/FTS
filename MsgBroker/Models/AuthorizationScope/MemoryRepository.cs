using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace MsgBroker.Models.AuthorizationScope
{
    public class MemoryRepository
    {
        // singleton
        private static MemoryRepository _instance;

        private string _path = string.Empty;
        public static MemoryRepository Instance
        {
            get { return _instance ??= new MemoryRepository(); }
        }


        private readonly ConcurrentBag<UserInfo> _repo;
        private string _adminLogin, _adminPassword;

        public MemoryRepository()
        {
            _repo = new ConcurrentBag<UserInfo>();
        }

        public void Init(string path)
        {
            _path = path;
            var xs = new XmlSerializer(typeof(AllUsers));
            if (string.IsNullOrWhiteSpace(path)) throw new Exception("path to user info file does not provide");
            
            if (!File.Exists(path))
            {
                var t = new AllUsers{AdminLogin="Admin", AdminPassword="Admin"};
                using StreamWriter sw = new StreamWriter(path);
                xs.Serialize(sw,t);
            }
            AllUsers info;
            using (var sr = new StreamReader(path))
                info = (AllUsers)xs.Deserialize(sr);
            if (info == null) throw new Exception("No users info...");
            foreach (var user in info.Users) _repo.Add(user);
            _adminLogin = info.AdminLogin;
            _adminPassword = info.AdminPassword;
        }

        public IQueryable<UserInfo> Items => _repo.AsQueryable();

        public UserInfo Get(string login, string password)
        {
            foreach (var item in Items)
            {
                if (item.Login == login && item.Password == password) return item;
            }
            return null;
        }

        public UserInfo GetAdminInfo() => new UserInfo(_adminLogin, _adminPassword);

        public bool Add(AuthorizationInfo ainfo)
        {
            if (ainfo == null)
                return false;
            foreach (var item in Items)
                if (item.Login == ainfo.Login && item.Password == ainfo.Password)
                    return false;
            _repo.Add(new UserInfo(ainfo.Login,ainfo.Password));
            SaveUsers();
            return true;
        }

        public void ChangeAdminCredentials(AuthorizationInfo ainfo)
        {
            _adminLogin = ainfo.Login.Trim();
            _adminPassword = ainfo.Password.Trim();
            SaveUsers();
        }

        private void SaveUsers()
        {
            if (File.Exists(_path)) File.Delete(_path);
            var xs = new XmlSerializer(typeof(AllUsers));
            var t = new AllUsers { AdminLogin = _adminLogin, AdminPassword = _adminPassword, Users = _repo.ToList() };
            using var sw = new StreamWriter(_path);
            xs.Serialize(sw, t);
        }
    }
}
