using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Core
{
    public static class DictionaryExtension
    {
        public static void AddOrUpdate(this Dictionary<string, string> dictionary, string key, string value )
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key,value);
            }
        }
    }
}
