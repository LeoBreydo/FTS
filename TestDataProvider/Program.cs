using System;
using System.Threading;
using LocalCommunicationLib;


namespace TestDataProvider
{
    static class Program
    {
        static readonly string[] Destinations = {
            "Service","Exchange","Market","Strategy"
        };
        static readonly string[] Codes = {
            "NoRestriction","SoftStop","HardStop"
        };
        static void Main()
        {
            Console.WriteLine("Starting Server");

            var ps = new PipeServer(
                new ServerStateProducer());

            ps.MessageReceivedEvent += (sender, args) =>
            {
                Console.WriteLine($"Command : Destination - {Destinations[args.Destination]}, DestID -  {args.DestinationId}, Code -  {Codes[args.RestrictionCode]}");
            };


            ps.Start();
            Thread.Sleep(100);
            Console.WriteLine("To exit hit any key...");
            Console.ReadLine();
            ps.Stop();
            Thread.Sleep(100);
        }
    }
}
