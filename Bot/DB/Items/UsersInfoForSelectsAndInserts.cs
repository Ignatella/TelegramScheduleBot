using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.DB.Items
{
    public struct UsersInfoForSelectsAndInserts
    {
        public string chat_id { get; }
        public UsersInfoForSelectsAndInserts(string chat_id)
        {
            this.chat_id = chat_id;
        }

        public static implicit operator UsersInfoForSelectsAndInserts(UsersInfoForRegistration info)
        {
            return new UsersInfoForSelectsAndInserts(chat_id: info.chat_id);
        }

        public static implicit operator UsersInfoForSelectsAndInserts(UsersInfoForResetting info)
        {
            return new UsersInfoForSelectsAndInserts(chat_id: info.chat_id);
        }

        public override string ToString()
        {
            return $"[chat_id: {chat_id}]";
        }
    }
}