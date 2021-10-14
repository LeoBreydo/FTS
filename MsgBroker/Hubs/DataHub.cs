using System.Threading.Tasks;
using LocalCommunicationLib;
using Microsoft.AspNetCore.SignalR;
using MsgBroker.Services;

namespace MsgBroker.Hubs
{
    public class DataHub : Hub
    {
        public static TSProxyService TsProxy { get; set; }
        public Task JoinAccount(string exchangeName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, exchangeName);
        }

        public Task LeaveAccount(string exchangeName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, exchangeName);
        }
       
        public void PostUserCommand(int[] args)
        {
            TsProxy.PostUserCommand(new () { DestinationId = args[0], RestrictionCode = args[1], Destination = args[2] });
        }
    }
}
