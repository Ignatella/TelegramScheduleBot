using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.InlineKeyboards
{
    public abstract class InlineKeyboard<T>
    {
        protected InlineKeyboardMarkup keyboard;
    
        protected InlineKeyboardButton[][] inlineKeyboardButtons;
        public int MessageID { get; set; }

        public InlineKeyboardMarkup Keyboard
        {
            get
            {
                keyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);
                return keyboard;
            }
        }
        virtual protected void FormKeyboard(T info) { }
   
    }   
}
