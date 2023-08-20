using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Amnhac.Api
{
    public class CharUtil
    {
        private static readonly string CHARACTER = "qwertyuiopasdfghjklzxcvbnm0123456789";
        private static string AES_KEY;
        private static string SALT;
        private static readonly string[] UniText = { "áàảãạâấầẩẫậăắằẵẳặ",
                    "éèẽẻẹêếềểễệ", "đ", "íìỉĩị",
                "óòỏõọôốồổỗộơớờởỡợ",
                "úùủũụưứừửữự", "ýỳỹỷỵ"};
        private static readonly string[] NouniText = { "a", "e", "d", "i", "o", "u", "y" };

        /// <summary>
        /// Encrypt given string to AES Base64 string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AesEnc(string s)
        {
            var b = Encoding.UTF8.GetBytes(AES_KEY);
            var iv = Encoding.UTF8.GetBytes(SALT);
            using (var alg = Aes.Create())
            {
                using (var encryptor = alg.CreateEncryptor(b, iv))
                {
                    using (var ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(s);
                        }
                        var result = ms.ToArray();
                        return Convert.ToBase64String(result);
                    }
                }
            }
        }
        
        /// <summary>
        /// Decrypt given Aes Encrypted in Base64 string to raw string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AesDec(string s)
        {
            var input = Convert.FromBase64String(s);
            var b = Encoding.UTF8.GetBytes(AES_KEY);
            var iv = Encoding.UTF8.GetBytes(SALT);
            string result = "";
            using (var alg = Aes.Create())
            {
                using (var decryptor = alg.CreateDecryptor(b, iv))
                {
                    using (var ms = new MemoryStream(input))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        using (var sr = new StreamReader(cs))
                        {
                            result = sr.ReadToEnd();
                        }
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Convert Unicode to Non-Unicode in Vietnamese
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ConvertToNonUnicode(string source)
        {
            for (int i=0; i< UniText.Length; i++)
            {
                for (int j = 0; j < UniText[i].Length; j++)
                {
                    source = source.Replace(UniText[i][j].ToString().ToUpper(), NouniText[i].ToUpper());
                    source = source.Replace(UniText[i][j].ToString(), NouniText[i]);
                }
            }
            return source;
        }

        /// <summary>
        /// Generate a random name with random length
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static string GetRandomName(int min, int max)
        {
            var rnd = new Random();
            int length = rnd.Next(min, max);
            string result = "";
            while (result.Length < length)
            {
                int num = rnd.Next(0, CHARACTER.Length - 1);
                char c = CHARACTER[num];
                if (result.Length == 0 && char.IsNumber(c))
                { }
                else
                {
                    result = result + c.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Hash your given string with key in HMACSHA512
        /// </summary>
        /// <param name="source"></param>
        /// <param name="secret_key"></param>
        /// <returns></returns>
        public static string PasswordHash(string source, string secret_key)
        {
            byte[] key = Encoding.UTF8.GetBytes(secret_key);
            using (HMACSHA512 hmac = new HMACSHA512(key))
            {
                byte[] result = hmac.ComputeHash(Encoding.UTF8.GetBytes(source));
                return BitConverter.ToString(result).Replace("-", "").ToLower();
            }

        }

        /// <summary>
        /// Compare string with smart search
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCompare"></param>
        /// <param name="threshold"></param>
        /// <param name="detailSearch"></param>
        /// <returns></returns>
        public static bool SmartCompare(string source, string toCompare, int threshold=0, bool detailSearch=false)
        {
            string s = source.ToLower();
            string l = toCompare.ToLower();
            if (source.Length> toCompare.Length)
            {
                s = toCompare.ToLower();
                l = source.ToLower();
            }
            if (threshold > s.Length / 2)
                threshold = s.Length / 2;
            int count = 0;
            int j = 0;
            if (l.Contains(s))
                return true;
            if (detailSearch==true)
            {
                s = s.Trim();
                if (s.Contains(" "))
                {
                    string[] word = s.Split(" ");
                    int wordCount = 0;
                    for (wordCount = 0; wordCount < word.Length; wordCount++)
                        if (!l.Contains(word[wordCount]))
                            break;
                    if (wordCount >= word.Length)
                        return true;
                }
            }
            for (int i=0; i< s.Length; i++)
            {
                int flag = count;
                while (j < l.Length && flag==count)
                {
                    if (s[i].Equals(l[j]))
                        count++;
                    j++;
                }
                if (j >= l.Length || count>=s.Length)
                    break;
            }
            if (count >= s.Length-threshold)
                return true;
            return false;
        }

        public static string Key { set {
                AES_KEY = value;
            }
        }
        public static string Salt
        {
            set
            {
                SALT = value;
            }
        }
    }
}
