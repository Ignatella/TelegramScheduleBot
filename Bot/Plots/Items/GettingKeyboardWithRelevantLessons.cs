using Bot.DB;
using Bot.DB.Items;
using Bot.InlineKeyboards;
using Bot.Lessons;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Linq;

namespace Bot.Plots.Items
{
    public static class GettingKeyboardWithRelevantLessons
    {
        public enum SetAction
        {
            InsertAndGet,
            Get
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static InlineKeyboardForFreeWeekDay inlineKeyboardForSchedule;

        #region MainPublicMethods
        public static InlineKeyboardForFreeWeekDay GetKeyboardWithRelevantLessons(Telegram.Bot.Args.MessageEventArgs e,
            bool CheckIfUserExists = false)
        {
            UsersInfoForSelectsAndInserts person =
                new UsersInfoForSelectsAndInserts(chat_id: e.Message.Chat.Id.ToString());

            logger.Info($"In GetKeyboardWithRelevantLessons function. user info:" +
                $" {e.Message.From.FirstName} {e.Message.From.LastName} {e.Message.From.Id}");

            try
            {
                TodayLessons lessons;

                lessons = MySQL.GetTodaySchedule(person, Program.todayDayName, CheckIfUserExists);

                if (lessons != null)
                {
                    if (lessons.NumOfLessonToday == 0)
                        return GetKeyboardForFreeWeekDay(e);

                    inlineKeyboardForSchedule = new InlineKeyboardForSchedule(lessons);

                    logger.Info($"No lessons today keyboard was sent to " +
                        $"{e.Message.From.FirstName} {e.Message.From.LastName} {e.Message.From.Id}.");
                }
            }
            catch (MySqlException ex)
            {
                logger.Error($"Error occured in GettingKeyboardWithRelevantLessons, with users data = {person}" +
                    $"and with checkIfUserExists = {CheckIfUserExists}" + 
                    $"and exception text = {ex}");
            }


            if (inlineKeyboardForSchedule == null)
            {
                Bot.botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: "You aren't registered!");
                e.Message.Text = "/start";
                logger.Info("Users isn't registered");
                return null;
            }

            SendWordSchedule(e);

            logger.Info($"Keyboard for schedule was sent to" +
                $" {e.Message.From.FirstName} {e.Message.From.LastName} {e.Message.From.Id} ");
            return inlineKeyboardForSchedule;
        }

        public static InlineKeyboardForFreeWeekDay GetKeyboardWithRelevantLessonsForAutomaticRefresh(
            string chatID)
        {

            UsersInfoForSelectsAndInserts person =
               new UsersInfoForSelectsAndInserts(chat_id: chatID);

            logger.Info($"In GetKeyboardWithRelevantLessonsForAutomaticRefresh function. user info:{chatID}");

            try
            {
                TodayLessons lessons;

                lessons = MySQL.GetTodaySchedule(person, Program.todayDayName, false);

                if (lessons.NumOfLessonToday == 0)
                    return new InlineKeyboardForFreeWeekDay();
                logger.Info($"No lessons today keyboard was sent to " +
                        $"{chatID}.");

                inlineKeyboardForSchedule = new InlineKeyboardForSchedule(lessons);
            }
            catch (MySqlException ex)
            {
                logger.Error($"Error occured in GettingKeyboardWithRelevantLessons, with users data = {person}" +
                    $"and with and exception text = {ex}");
            }

            logger.Info($"Keyboard for schedule was sent to {chatID}");
            return inlineKeyboardForSchedule;
        }

        public static InlineKeyboardForFreeWeekDay EndRegistrationGetKeyboardWithRelevantLessons(
            Telegram.Bot.Args.MessageEventArgs e,
            string[] Name_Of_Choosen_Objects = null)
        {
            UsersInfoForSelectsAndInserts person =
                new UsersInfoForSelectsAndInserts(chat_id: e.Message.Chat.Id.ToString());

            logger.Info($"In EndRegistrationGetKeyboardWithRelevantLessons function. user info:" +
               $" {e.Message.From.FirstName} {e.Message.From.LastName} {e.Message.From.Id}");

            try
            {
                TodayLessons lessons;

                lessons = MySQL.InsertingNonRequiredLessonsAndGettingTodaySchedule(
                        Name_Of_Choosen_Objects, person, Program.todayDayName);

                if (lessons.NumOfLessonToday == 0)
                    return GetKeyboardForFreeWeekDay(e);
           
                inlineKeyboardForSchedule = new InlineKeyboardForSchedule(lessons);
             
            }
            catch (MySqlException ex)
            {
                logger.Error($"Error occured in EndRegistrationGetKeyboardWithRelevantLessons, " +
                    $"with users data = {person}" +
                    $"and with NameOfChoosenObjects = {string.Join(',', Name_Of_Choosen_Objects)} " +
                    $"and exception text = {ex}");
            }

            SendWordSchedule(e);

            logger.Info($"Keyboard for schedule was sent to" +
                $" {e.Message.From.FirstName} {e.Message.From.LastName} {e.Message.From.Id} ");

            return inlineKeyboardForSchedule;
        }

        #endregion

        #region PrivateSupportingMethods
        private static InlineKeyboardForFreeWeekDay GetKeyboardForFreeWeekDay(Telegram.Bot.Args.MessageEventArgs e)
        {
            var KeyboardForFreeWeekDay = new InlineKeyboardForFreeWeekDay();

            KeyboardForFreeWeekDay.MessageID = Bot.botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: "WOW, you don't have lessons today!",
                replyMarkup: KeyboardForFreeWeekDay.Keyboard).Result.MessageId;

            return KeyboardForFreeWeekDay;
        }
        private static void SendWordSchedule(Telegram.Bot.Args.MessageEventArgs e)
        {
            inlineKeyboardForSchedule.MessageID = Bot.botClient.SendTextMessageAsync(
                     chatId: e.Message.Chat,
                     text: "Schedule:",
                     replyMarkup: inlineKeyboardForSchedule.Keyboard).Result.MessageId;
        }
        #endregion
    }
}
