using System;
using System.IO;
using System.Xml.Serialization;

namespace BrokerFacadeIB
{
    public class IBCredentials
    {
        public string Location;
        public string Login;
        public string Password;

        public string Hostname = "127.0.0.1";
        public int Port = 7497;
        public int ClientId = 1;

        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(IBCredentials));
        public static IBCredentials Restore(string fileName)
        {
            try
            {
                if (!File.Exists(fileName)) return null;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return (IBCredentials) serializer.Deserialize(fs);
                }
            }
            catch
            {
                return null;
            }
            
        }
        public string Save(string fileName)
        {
            try
            {
                string folder = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (folder != null && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using (TextWriter writer = new StreamWriter(fileName))
                {
                    serializer.Serialize(writer, this);
                }

                return null;
            }
            catch(Exception exception)
            {
                return exception.Message;
            }
        }
    }
}