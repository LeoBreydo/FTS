using System;

namespace CoreTypes
{
    public interface ICommand
    {
        CommandDestination Destination { get; }
        CommandSource Source { get; }
        int DestinationId { get; }
    }

    public class RestrictionCommand : ICommand
    {
        public CommandDestination Destination { get; }
        public CommandSource Source { get; }
        public int DestinationId { get; }
        public TradingRestriction Restriction { get; }

        public RestrictionCommand(CommandDestination destination, CommandSource source, 
            int destinationId, TradingRestriction restriction)
        {
            Destination = destination;
            Source = source;
            DestinationId = destinationId;
            Restriction = restriction;
        }
    }
    public class OrderForgetCommand : ICommand
    {
        public CommandDestination Destination { get; } = CommandDestination.Strategy;
        public CommandSource Source { get; } = CommandSource.User;
        public int DestinationId { get; }

        public OrderForgetCommand(int destinationId)
        {
            DestinationId = destinationId;
        }
    }
    public class ErrorsForgetCommand : ICommand
    {
        public CommandDestination Destination { get; }
        public CommandSource Source { get; } = CommandSource.User;
        public int DestinationId { get; }

        public ErrorsForgetCommand(CommandDestination destination, int destinationId)
        {
            Destination = destination;
            DestinationId = destinationId;
        }
    }

    public class OrderRepeatCommand : ICommand
    {
        public CommandDestination Destination { get; } = CommandDestination.Strategy;
        public CommandSource Source { get; } = CommandSource.User;
        public int DestinationId { get; }

        public OrderRepeatCommand(int destinationId)
        {
            DestinationId = destinationId;
        }
    }

    public class ManualFillCommand : ICommand
    {
        public CommandDestination Destination { get; } = CommandDestination.Strategy;
        public CommandSource Source { get; } = CommandSource.User;
        public int DestinationId { get; }
        public int SignedAmount { get; }
        public decimal Price { get; }
        public DateTime DateTime { get; } = DateTime.UtcNow;
        public int OrderId { get; } = -1;
        public string ExecId { get; } = "manual";

        public ManualFillCommand(int destinationId, int signedAmount, decimal price)
        {
            DestinationId = destinationId;
            SignedAmount = signedAmount;
            Price = price;
        }
    }

    public enum CommandDestination
    {
        Service = 0,
        Exchange = 1,
        Market = 2,
        Strategy = 3
    }

    public enum CommandSource : int
    {
        User = 0,
        Scheduler = 1,
        CriticalLoss = 2,
        Parent = 3,
        EndOfContract = 4,
        EndOfSession = 5,
        Error = 6,
        ErrorsNbr = 7
    }
    public static class RSourceEx
    {
        public static int ToInt(this CommandSource r)
        {
            return (int)r;
        }
        public static string AsString(this CommandSource r)
        {
            return r switch
            {
                CommandSource.User => "User",
                CommandSource.Scheduler => "Scheduler",
                CommandSource.CriticalLoss => "CriticalLoss",
                CommandSource.Parent => "Parent",
                CommandSource.EndOfContract => "EndOfContract",
                CommandSource.EndOfSession => "EndOfSession",
                CommandSource.Error => "Error",
                _ => "ErrorsNbr"
            };
        }
    }

    public interface ICommandReceiver
    {
        void ApplyCommand(ICommand command);
    }
}
