using Bot.DB;
using Bot.DB.Items;
using Bot.Lessons;
using Bot.Lessons.Items;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.InlineKeyboards
{
    public class InlineKeyboardForSchedule : InlineKeyboardForFreeWeekDay
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties
        private TodayLessons TodayLessons { get; }
       
        #endregion

        #region Constructors
        public InlineKeyboardForSchedule(TodayLessons todayLessons)
        {
                logger.Trace("In Inline Keyboard For Schedule constructor.");

            inlineKeyboardButtons = new InlineKeyboardButton[6][];
            NextDayID = (int)Enum.Parse(typeof(DayOfWeek), Program.todayDayName);
            TodayLessons = todayLessons;
            FormKeyboard(TodayLessons.RelevantLesson);
        }
        #endregion

        #region Methods
      
        public void Refresh()
        {
            logger.Info("Refresh button has been pressed");

            FormKeyboard(TodayLessons.RelevantLesson);
            NextDayID = (int)Enum.Parse(typeof(DayOfWeek), Program.todayDayName);
        }

        public void NextLesson()
        {
            logger.Info("Next Lesson button has been pressed");

            FormKeyboard(TodayLessons.NextLesson);
        }

        protected override void FormKeyboard(Atomic_Lesson lesson)
        {
            inlineKeyboardButtons[0] =
                new[] { new InlineKeyboardButton { Text = lesson.DayOfWeek.ToUpper(), CallbackData = "_" },
                        new InlineKeyboardButton { Text = TodayLessons.NumOfLessonToday.ToString(), CallbackData = "_" },
                        new InlineKeyboardButton { Text = lesson.LessonID.ToString(), CallbackData = "_"} };

            inlineKeyboardButtons[1] =
                new[] { new InlineKeyboardButton { Text = lesson.LessonName, CallbackData = "_" } };

            inlineKeyboardButtons[2] =
                new[] { new InlineKeyboardButton { Text = $"{lesson.Starts:t}", CallbackData = "_" } ,
                        new InlineKeyboardButton { Text = $"{lesson.Ends:t}", CallbackData = "_" } };

            inlineKeyboardButtons[3] =
                new[] { new InlineKeyboardButton { Text = lesson.Auditorium, CallbackData = "_" } };

            inlineKeyboardButtons[4] =
                new[] { new InlineKeyboardButton { Text = lesson.RemainedTime == TimeSpan.Zero 
                    || DateTime.Now.DayOfWeek.ToString() != Program.todayDayName
                        ? "--:--" : $"{lesson.RemainedTime:hh\\:mm}", CallbackData = "_" } };
            
            inlineKeyboardButtons[5] =
                new[] { new InlineKeyboardButton { Text = "\U000023E9", CallbackData = "NextDay" },
                        new InlineKeyboardButton { Text = "\U0001F504", CallbackData = "Refresh" },
                        new InlineKeyboardButton { Text = "\U000025B6", CallbackData = "NextLesson" } };

            logger.Trace($"Keyboard formed with data:{lesson}");
        }

        #endregion
    }
}