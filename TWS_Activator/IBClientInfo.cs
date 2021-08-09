using System.IO;
using System.Reflection;
using Utilities;

namespace TWS_Activator
{
    public class IBClientInfo
    {
        public string ClientLocation;
        public string Login;
        public string Password;

        public bool IsValid()
        {
            return File.Exists(ClientLocation) &&
                   !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private static string GetFileName()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "",
                "TWS_Activator.credentials.xml");
        }
        public static IBClientInfo Load()
        {
            return Serializer<IBClientInfo>.Open(GetFileName());
        }

        public void Save()
        {
            Serializer<IBClientInfo>.Save(this, GetFileName());
        }

        public static void DeleteSaved()
        {
            PathEx.TryDeleteFile(GetFileName());
        }
    }
}
