using System;
using System.IO;
using BrokerFacadeIB;

namespace Driver
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (!MainObject.Create(ReadIBCredentials(), out MainObject mo, out string error))
            {
                Console.WriteLine("Failed to initialize trading service: " + error);
                Console.WriteLine("To exit hit any key");
                Console.ReadKey();
                return;
            }

            mo.StartWork();
            Console.ReadKey();
            mo.StopWork();

            Console.WriteLine("To exit hit any key");
            Console.ReadKey();
        }

        static IBCredentials ReadIBCredentials()
        {
            const string Credentials_FileName = "IbCredentials.xml";
            var cred = IBCredentials.Restore(Credentials_FileName);
            if (cred == null)
            {
                string login = ReadNotEmptyLine("Enter TWS login:");
                string pwd = ReadNotEmptyLine("Password:");
                string path = @"C:\Jts\tws.exe";
                if (!File.Exists(path))
                {
                    Console.WriteLine($"TWS application not found by default location '{path}'");
                    path = ReadNotEmptyLine("Enter full path to Tws application",
                        pt =>
                        {
                            if (!string.IsNullOrEmpty(pt) && File.Exists(pt) &&
                                string.Equals(Path.GetExtension(pt), ".exe", StringComparison.OrdinalIgnoreCase))
                                return true;

                            Console.WriteLine("Invalid path");
                            return false;
                        });
                }

                cred = new IBCredentials {Login = login, Password = pwd, Location = path};
                cred.Save(Credentials_FileName);
            }

            return cred;
        }

        static string ReadNotEmptyLine(string prompt,Func<string,bool> check=null)
        {
            if (check == null)
                check = arg => !string.IsNullOrEmpty(arg);
            while (true)
            {
                Console.Write(prompt);
                string ret = Console.ReadLine();
                if (check(ret)) return ret;
            }
        }
    }
}
