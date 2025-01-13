using System.Security.Cryptography;
using System.Text;

namespace XGuard
{
    public class HashingUtility
    {
        public static string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Преобразуем байты в шестнадцатеричный формат
                }
                return builder.ToString();
            }
        }
    }
}
