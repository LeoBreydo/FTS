using ProtoBuf;

namespace LocalCommunicationLib
{
    [ProtoContract]
    public class UserCommand
    {
        [ProtoMember(1)]
        public int Destination;
        [ProtoMember(2)]
        public int DestinationId;
        [ProtoMember(3)]
        public int RestrictionCode;
        // destination : 0-global, 1 - exchange, 2 - market, 3 - strategy
        // restriction code : 0 - start, 1 - softStop, 2 - hardStop
    }
}
