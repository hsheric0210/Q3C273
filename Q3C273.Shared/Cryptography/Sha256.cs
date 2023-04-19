using System.Security.Cryptography;
using System.Text;

namespace Q3C273.Shared.Cryptography
{
    public static class Sha256
    {
        public static string ComputeHash(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);

            using (var sha = new SHA256Managed())
            {
                data = sha.ComputeHash(data);
            }

            var hash = new StringBuilder();

            foreach (var _byte in data)
                hash.Append(_byte.ToString("X2"));

            return hash.ToString().ToUpper();
        }

        public static byte[] ComputeHash(byte[] input)
        {
            using (var sha = new SHA256Managed())
            {
                return sha.ComputeHash(input);
            }
        }
    }
}
