using System;

namespace LocalCommunicationLib
{
    public class MessageReceivedEventArgs : EventArgs
    {
        // message source - always User
        public int Destination { get; set; } // system, exchange, market, strategy
        public int DestinationId { get; set; }
        public int RestrictionCode { get; set; } // NoRestrictions, SoftStop, HardStop
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public string ClientId { get; set; }
        public bool IsStateProvider { get; set; }
    }

    public class ClientDisconnectedEventArgs : EventArgs
    {
        public string ClientId { get; set; }
    }
}
