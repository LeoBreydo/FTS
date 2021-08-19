using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Binosoft.TraderLib.Indicators;
using BrokerFacadeIB;

namespace Driver
{
    static class Program
    {
        static void Main(string[] args)
        {
            ReadIBCredentials();
            IndicatorsServer.Init(GetIndicatorsFolder());

            var cancellationTokenSource = new CancellationTokenSource();
            var t = MainLoop(cancellationTokenSource);


            Console.ReadKey();
            cancellationTokenSource.Cancel();
            t.Wait();
            Console.WriteLine("To exit hit any key");
            Console.ReadKey();
        }

        private static IBCredentials _ibCredentials;
        static void ReadIBCredentials()
        {
            const string Credentials_FileName = "IbCredentials.xml";
            _ibCredentials = IBCredentials.Restore(Credentials_FileName);
            if (_ibCredentials == null)
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

                _ibCredentials = new IBCredentials {Login = login, Password = pwd, Location = path};
                _ibCredentials.Save(Credentials_FileName);
            }
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
        private static string GetIndicatorsFolder()
        {
            var path = Path.GetFullPath("Indicators");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private static Task MainLoop(CancellationTokenSource cts)
        {
            return Task.Factory.StartNew((_) =>
                {
                    var mo = new MainObject(_ibCredentials);
                    Thread.Sleep(1000);
                    Console.WriteLine("Started?");
                    var ms = (int)Math.Floor(DateTime.UtcNow.TimeOfDay.TotalMilliseconds);
                    try
                    {
                        while (true)
                        {
                            if (cts.Token.IsCancellationRequested)
                            {
                                //Console.WriteLine("Throwed");
                                mo.PlaceStopRequest();
                                var elapsedSeconds = 0;
                                while (!mo.IsReadyToBeStooped)
                                {
                                    if (elapsedSeconds >= 120) break;
                                    Thread.Sleep(5000);
                                    elapsedSeconds += 5;
                                }
                                mo.Facade.Stop();
                                mo.Logger.Flush();
                                if(elapsedSeconds < 120)
                                    throw new TaskCanceledException("System will be stopped properly.");
                                throw new TaskCanceledException("System can be stopped properly, it will be stopped anyway.");
                            }

                            var dt = DateTime.UtcNow;
                            mo.DoWork(dt);
                            var cms = (int)Math.Floor(dt.TimeOfDay.TotalMilliseconds);
                            var lastWorkDuration = cms - ms;
                            if (lastWorkDuration > 1000) 
                            {
                                
                                ms = ++cms;
                                Thread.Sleep(1);
                            }
                            else
                            {
                                var sleepTime = 1000 - (cms % 1000);
                                ms += sleepTime;
                                Thread.Sleep(sleepTime);
                            }
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        cts.Dispose();
                    }
                }
                , TaskCreationOptions.LongRunning
                , cts.Token);
        }
    }
}
