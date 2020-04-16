using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using NLog;

namespace Bot.InlineKeyboards
{
   
    public class InlineKeyboardForRegistration : InlineKeyboard<string[]> //здесь тоже есть логгер.
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties

        private string[] Text_For_Buttons { get; set; }
        public List<string> Name_Of_Choosen_Objects { get; private set; }

        #endregion

        #region Constructors
        public InlineKeyboardForRegistration(params string[] Text_For_Buttons)
        {
            logger.Debug("In Inline Keyboard For Registration constructor.");

            this.Text_For_Buttons = Text_For_Buttons;

            inlineKeyboardButtons = new InlineKeyboardButton[Text_For_Buttons.Length][];

            FormKeyboard(this.Text_For_Buttons);

            Name_Of_Choosen_Objects = new List<string>();
        }
        #endregion
        
        #region Methods 
        public void UpdateKeybordChoosen(string Data_Of_Object_to_Change)
        {
            int indexOfObject = Array.IndexOf(Text_For_Buttons, Data_Of_Object_to_Change);
            if (indexOfObject < 0) throw new ArgumentOutOfRangeException("Keyboard doesn't consist such word");

            if (Name_Of_Choosen_Objects.Contains(Data_Of_Object_to_Change))
            {
                this.inlineKeyboardButtons[indexOfObject][0].Text = Data_Of_Object_to_Change;
                Name_Of_Choosen_Objects.Remove(Data_Of_Object_to_Change);
            }
            else
            {
                this.inlineKeyboardButtons[indexOfObject][0].Text += " \U00002705";
                Name_Of_Choosen_Objects.Add(Data_Of_Object_to_Change);
            }
            logger.Trace($"Keyboard for registration formed with data: {Data_Of_Object_to_Change}");
        }

        protected override void FormKeyboard(string[] info)
        {
            for (int i = 0; i < info.Length; i++)
            {
                inlineKeyboardButtons[i] =
                    new[] { new InlineKeyboardButton { Text = info[i], CallbackData = info[i] } };
            }

            logger.Trace($"Keyboard for registration formed with data: {string.Join(';', info)}");
        }

        #endregion
    }
}
