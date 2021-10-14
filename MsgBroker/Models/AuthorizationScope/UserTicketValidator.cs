using MsgBroker.Models.Common;

namespace MsgBroker.Models.AuthorizationScope
{
    public static class UserTicketValidator
    {
        public const string __key = "my_key_is_so_short...";
        public static MemoryRepository Repository { get; set; }

        public static bool ValidateTicket(string ut)
        {
            var ticket = UserTicket.Decrypt(ut, __key);
            var ainfo = new AuthorizationInfo { Login = ticket.Login, Password = ticket.Password };
            return LoginVerificator.VerifyCredentials(ainfo, Repository);
        }
    }
}
