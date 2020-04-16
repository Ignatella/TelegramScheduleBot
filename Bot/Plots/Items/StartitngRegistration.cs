using Bot.DB;
using Bot.DB.Items;
using Bot.InlineKeyboards;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Plots
{
    public class StartitngRegistration
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        InlineKeyboardForRegistration inlineKeyboardForRegistration;

        public InlineKeyboardForRegistration UserSelectedGroup(Telegram.Bot.Args.MessageEventArgs e)
        {
            UsersInfoForRegistration person = new UsersInfoForRegistration(
                chat_id: e.Message.Chat.Id.ToString(),
                group_name: e.Message.Text,
                first_name: e.Message.From.FirstName,
                last_name: e.Message.From.LastName,
                user_name: e.Message.From.Username);

            logger.Info($"In UserSelectedGroup with " +
                $"user data {e.Message.From.FirstName} {e.Message.From.LastName} {e.Message.From.Id} ");

            try
            {
                inlineKeyboardForRegistration =
                    new InlineKeyboardForRegistration(MySQL.UserRegistrationOrUpdatingAndGettingNonRequiredLessons(person));
            }
            catch (MySqlException ex)
            {
                logger.Error($"Error occured in Bot_UserSelectedGroupHanldler in StartingRegistration " +
                    $"with persons data: {person}" +
                    $"with exception data: {ex}");
                return null;
            }

            inlineKeyboardForRegistration.MessageID = Bot.botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: "Please, choose you lessons and press READY button:",
                replyMarkup: inlineKeyboardForRegistration.Keyboard).Result.MessageId;

            Bot.botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: "And press READY button",
                 replyMarkup: new ReplyKeyboardMarkup
                 {
                    Keyboard = new KeyboardButton[][]
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton{Text="READY"}
                        }
                    },
                     ResizeKeyboard = true,
                     OneTimeKeyboard = true
                 });

            logger.Info("InlineKeyboardForRegistration was sent to user.");
            return inlineKeyboardForRegistration;
        }
    }
}
