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
                                Console.WriteLine("Throwed");
                                // verify all positions are closed!!
                                mo.Stop();
                                Thread.Sleep(5000);
                                throw new TaskCanceledException("Request to stop");
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
