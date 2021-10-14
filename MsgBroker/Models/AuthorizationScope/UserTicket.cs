using System;
using System.Security.Cryptography;
using System.Text;

namespace MsgBroker.Models.AuthorizationScope
{
    public class UserTicket
    {
        public readonly string Login;
        public readonly string Password;

        public UserTicket(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login)) throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
            if (!System.Text.RegularExpressions.Regex.IsMatch(login, @"^[a-zA-Z0-9]+$"))
                throw new Exception("Only alphanumeric strings are accepted");
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
                throw new Exception("Only alphanumeric strings are accepted");
            Login = login;
            Password = password;
        }

        public static bool VerifyId(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9]+$");
        }

        public string Encrypt(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            return Convert.ToBase64String(Encoding.UTF8.GetBytes((Login + "|" + Password).Encrypt(key)));
        }

        public static UserTicket Decrypt(string crypted, string key)
        {
            if (string.IsNullOrWhiteSpace(crypted)) throw new ArgumentNullException(nameof(crypted));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(crypted);
                var decrypted = Encoding.UTF8.GetString(base64EncodedBytes).Decrypt(key);
                if (decrypted == null) return null;
                var splits = decrypted.Split('|');
                if (splits.Length != 2) return null;
                string login = splits[0];
                string password = splits[1];
                return new UserTicket(login, password);
            }
            catch
            {
                return null;
            }
        }
    }

    public static class StringExtensionsForUserTicketUsing
    {
        /// <summary>
        /// Encryptes a string using the supplied key. Encoding is done using RSA encryption.
        /// </summary>
        /// <param name="stringToEncrypt">String that must be encrypted.</param>
        /// <param name="key">Encryptionkey.</param>
        /// <returns>A string representing a byte array separated by a minus sign.</returns>
        /// <exception cref="ArgumentException">Occurs when stringToEncrypt or key is null or empty.</exception>
        public static string Encrypt(this string stringToEncrypt, string key)
        {
            if (string.IsNullOrEmpty(stringToEncrypt))
            {
                throw new ArgumentException("An empty string value cannot be encrypted.");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot encrypt using an empty key. Please supply an encryption key.");
            }

            var cspp = new CspParameters();
            cspp.KeyContainerName = key;
            //cspp.Flags = CspProviderFlags.UseMachineKeyStore;
            var rsa = new RSACryptoServiceProvider(cspp) { PersistKeyInCsp = true };
            var bytes = rsa.Encrypt(System.Text.Encoding.UTF8.GetBytes(stringToEncrypt), true);

            return BitConverter.ToString(bytes);
        }

        /// <summary>
        /// Decryptes a string using the supplied key. Decoding is done using RSA encryption.
        /// </summary>
        /// <param name="stringToDecrypt">String that must be decrypted.</param>
        /// <param name="key">Decryptionkey.</param>
        /// <returns>The decrypted string or null if decryption failed.</returns>
        /// <exception cref="ArgumentException">Occurs when stringToDecrypt or key is null or empty.</exception>
        public static string Decrypt(this string stringToDecrypt, string key)
        {
            string result = null;

            if (string.IsNullOrEmpty(stringToDecrypt))
            {
                throw new ArgumentException("An empty string value cannot be encrypted.");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot decrypt using an empty key. Please supply a decryption key.");
            }

            try
            {
                var cspp = new CspParameters { KeyContainerName = key };
                var rsa = new RSACryptoServiceProvider(cspp) { PersistKeyInCsp = true };

                var decryptArray = stringToDecrypt.Split(new string[] { "-" }, StringSplitOptions.None);
                var decryptByteArray = Array.ConvertAll<string, byte>(decryptArray, (s => Convert.ToByte(byte.Parse(s, System.Globalization.NumberStyles.HexNumber))));


                byte[] bytes = rsa.Decrypt(decryptByteArray, true);
                result = System.Text.Encoding.UTF8.GetString(bytes);

            }
            finally
            {
                // no need for further processing
            }

            return result;
        }


    }
}
