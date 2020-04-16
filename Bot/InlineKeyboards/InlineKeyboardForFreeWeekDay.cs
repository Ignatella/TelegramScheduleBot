using Bot.DB;
using Bot.DB.Items;
using Bot.Lessons.Items;
using MySql.Data.MySqlClient;
using NLog;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.InlineKeyboards
{
   
    public class InlineKeyboardForFreeWeekDay : InlineKeyboard<Atomic_Lesson>, INextDayIndex
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Fieds

        private int nextDayID;

        #endregion

        #region Properties
        public int NextDayID
        {
            get
            {

                if (nextDayID >= (int)Enum.Parse(typeof(DayOfWeek), Program.WorkingDays[^1]))
                    nextDayID = 0;
                return ++nextDayID;

            }
            set
            {
                nextDayID = (int)Enum.Parse(typeof(DayOfWeek), Program.todayDayName); 
            }
        }

        public int NextDayMessageID { get; set; }
        #endregion

        #region Constructors
        public InlineKeyboardForFreeWeekDay()
        {
            logger.Info("In InlineKeyboardForFreeWeekDays constructor.");
            inlineKeyboardButtons = new InlineKeyboardButton[1][];
            FormKeyboard();
        }

        #endregion

        #region Methods

        public string NextDay(UsersInfoForSelectsAndInserts person)
        {
            logger.Info("NextDay button has been pressed");

            try
            {
                return MySQL.GetAnotherDayLessons(person, Enum.GetNames(typeof(DayOfWeek))[NextDayID]).ToString();
            }
            catch (MySqlException ex)
            {
                logger.Error($"Error occured in DB. Catched in NextDay() function. Data from NextDay {person} " +
                    $"and exception{ex}");

                return null;
            }
        }
      
        protected void FormKeyboard()
        {
            inlineKeyboardButtons[0] =
             new[] { new InlineKeyboardButton { Text = "\U000023E9", CallbackData = "NextDay" }};
        }
        #endregion
    }
}
