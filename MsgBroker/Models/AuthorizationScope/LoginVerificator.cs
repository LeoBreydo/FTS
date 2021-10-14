namespace MsgBroker.Models.AuthorizationScope
{
    public static class LoginVerificator
    {
        public static bool VerifyCredentials(AuthorizationInfo ainfo, MemoryRepository repo)
        {
            return ainfo != null && VerifyCredentials(ainfo.Login, ainfo.Password, repo);
        }

        private static bool VerifyCredentials(string login, string password, MemoryRepository repo)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) return false;

            login = login.Trim();
            password = password.Trim();
            return repo.Get(login, password) != null;
        }

        public static bool VerifyAdminCredentials(AuthorizationInfo ainfo, MemoryRepository repo)
        {
            return ainfo != null && VerifyAdminCredentials(ainfo.Login, ainfo.Password, repo);
        }

        private static bool VerifyAdminCredentials(string login, string password, MemoryRepository repo)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) return false;

            login = login.Trim();
            password = password.Trim();
            var ai = repo.GetAdminInfo();
            return ai.Login.Equals(login) && ai.Password.Equals(password);
        }
    }
}
