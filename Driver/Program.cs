using System;
using System.Threading;
using System.Threading.Tasks;

namespace Driver
{
    static class Program
    {
        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var t = MainLoop(cancellationTokenSource);


            Console.ReadKey();
            cancellationTokenSource.Cancel();
            t.Wait();
            Console.WriteLine("To exit hit any key");
            Console.ReadKey();

        }

        private static Task MainLoop(CancellationTokenSource cts)
        {
            return Task.Factory.StartNew((_) =>
                {
                    var mo = new MainObject();
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
