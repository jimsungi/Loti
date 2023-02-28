using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace TigerL10N.Service
{
    public class LocService
    {
        public static CultureInfo[] GetAllCultures()
        {
            CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
            return cinfo;
        }

        /// <summary>
        /// Generate auto key from the text
        /// </summary>
        /// <param name="recommandID"></param>
        /// <returns></returns>

        public static List<string> IdKey = new List<string>();
        public static List<string> StringKey = new List<string>();

        public static string GetRecommandID(string recommandID, bool id)
        {
            List<string> source_list = null;

            if (id == true)
                source_list = IdKey;
            else
                source_list = StringKey;

            string res = string.Empty;
            string key_str = recommandID;
            key_str = key_str.Replace("\r\n", "_");
            key_str = key_str.Replace("\"", "_");
            key_str = key_str.Replace("@", "at");
            key_str = key_str.Replace("/", "_");
            key_str = key_str.Replace(".", "_");
            key_str = key_str.Replace(" ", "_");
            key_str = key_str.Replace("=", "_");
            key_str = key_str.Replace(",", "_");
            key_str = key_str.Replace("[", "_");
            key_str = key_str.Replace("]", "_");
            key_str = key_str.Replace("<", "_");
            key_str = key_str.Replace(">", "_");
            key_str = key_str.Replace("!", "_");
            key_str = key_str.Replace("?", "_");
            key_str = key_str.Replace("+", "_");
            key_str = key_str.Replace("-", "_");
            key_str = key_str.Replace("(", "_");
            key_str = key_str.Replace(")", "_");
            key_str = key_str.Replace("'", "_");
            if (key_str.StartsWith("0"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("1"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("2"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("3"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("4"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("5"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("6"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("7"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("8"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("9"))
                key_str = "_" + key_str;
            if (key_str.Length == 0)
                key_str = "_";

            string base_str = key_str;
            int i = 0;
            while(source_list.Contains(base_str))
            {
                base_str = string.Format("{0}_{1}", key_str, i++);
            }
            source_list.Add(base_str);
            return "G." +base_str;
        }
    }
}
