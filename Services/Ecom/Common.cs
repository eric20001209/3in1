using eCommerce_API.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace eCommerce_API.Services
{
	public class Common
	{

        public static char[] constant = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        /// <summary>
        /// generate 0-z random string
        /// </summary>
        /// <param name="length">string length</param>
        /// <returns>Random String :)</returns>
        public static string GenerateRandomString(int length)
        {
            string checkCode = String.Empty;
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                checkCode += constant[rd.Next(36)].ToString();
            }
            return checkCode;
        }

        public static string ConvertoToMD5(string str)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            string md5 = sBuilder.ToString().ToUpper();
            return md5;
        }

        public static string ReplacePriceInDescription(string input, string regex)
        {
            return Regex.Replace(input, regex, "");
        }

        public static string RemoveString(string input, string symbol)
        {
            var start = input.IndexOf(symbol);
            string final = "";
            if (start > 0)
                final = input.Substring(0, start);
            else
                final = input;
            return final;
        }

    }
}
