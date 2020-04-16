using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.DB.Items
{
    public struct UsersInfoForRegistration
    {
        public string chat_id { get; }
        public string group_name { get; }
        public string first_name { get; set; } 
        public string user_name { get; set; }
        public string last_name { get; set; }

        public UsersInfoForRegistration(string chat_id, string group_name, string first_name, string last_name,
            string user_name)
        {
            this.chat_id = chat_id;
            this.first_name = first_name;
            this.last_name = last_name;
            this.user_name = user_name;
            this.group_name = group_name;
        }

        public override string ToString()
        {
            return $"[chat_id: {chat_id}; group_name: {group_name}; " +
                $"first_name: {first_name}; user_name: {user_name}, last_name: {last_name}]";
        }
    }
}