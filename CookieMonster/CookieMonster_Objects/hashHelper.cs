using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Statyczna klasa spinajaca generowanie MD5 itp.
    /// </summary>
    static class hashHelper
    {
        static public string CalculateMD5(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        static public string CalculateSHA1(string input)
        {
            SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = sha1.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// The method create a Base64 encoded string from a normal string.
        /// </summary>
        /// <param name="toEncode">The String containing the characters to encode.</param>
        /// <returns>The Base64 encoded string.</returns>
        public static string EncodeTo64(string toEncode)
        {

            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        /// <summary>
        /// The method to Decode your Base64 strings.
        /// </summary>
        /// <param name="encodedData">The String containing the characters to decode.</param>
        /// <returns>A String containing the results of decoding the specified sequence of bytes.</returns>
        public static string DecodeFrom64(string encodedData)
        {

            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            return System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);

        }
    }
}
