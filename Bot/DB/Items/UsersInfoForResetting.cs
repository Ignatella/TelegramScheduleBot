using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.DB.Items
{
    public struct UsersInfoForResetting
    {

        public string chat_id { get; }
        public string group_name { get; }

        public UsersInfoForResetting(string chat_id, string group_name)
        {
            this.chat_id = chat_id;
            this.group_name = group_name;
        }

        public static implicit operator UsersInfoForResetting(UsersInfoForRegistration info)
        {
            return new UsersInfoForResetting(chat_id: info.chat_id, group_name: info.group_name);
        }
        public override string ToString()
        {
            return $"[chat_id: {chat_id}; group_name: {group_name}]";
        }

    }
}