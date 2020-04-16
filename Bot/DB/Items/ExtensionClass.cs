using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Bot.DB.Items
{
    public static class ExtensionClass
    {
        public static string ArrayToStringWichCommaForMySQL(this string[] array, 
            string parametrName, MySqlCommand cmd)
        {

            if (array == null) throw new ArgumentNullException();

            List<string> partsForString = new List<string>();
            for (int i = 0; i < array.Length; i++)
            {
                string s = "@" + parametrName + i;
                cmd.Parameters.Add(s, MySqlDbType.VarChar).Value = array[i];
                partsForString.Add(s);
            }
            return string.Join(",", partsForString);
        }
    }
}