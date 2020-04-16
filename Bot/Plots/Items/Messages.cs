using Bot;
using Bot.DB;
using Bot.DB.Items;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Plots.Items
{
    public static class Messages
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static void SendСustomMessage(string msg, bool quite = true, 
            (bool CheckIfUserIsAdmin , Telegram.Bot.Types.Chat chat) dataForCheckingIfUserIsAdmin = 
            default((bool, Telegram.Bot.Types.Chat)))
        {

            (bool isAdmin, int[] ids) ids = default;

            try
            {
                if (dataForCheckingIfUserIsAdmin.CheckIfUserIsAdmin)
                {
                    var person = new UsersInfoForSelectsAndInserts(chat_id: dataForCheckingIfUserIsAdmin.chat.Id.ToString());
                    ids = MySQL.GetAllChatID((dataForCheckingIfUserIsAdmin.CheckIfUserIsAdmin, person));
                    logger.Info("User checked. in SendCustomMessage.");
                }
                else
                    ids = MySQL.GetAllChatID();
                logger.Info("User checked. in SendCustomMessage. Without checking if user is admin.");
            }
            catch(MySqlException ex)
            {
                logger.Error($"An error occured in SendCustomMessage in SendMessage with {msg} and exception {ex}");
            }

            if (!ids.isAdmin)
                return;

            foreach (var id in ids.ids)
            {
                if (quite)
                    Bot.botClient.SendTextMessageAsync(
                        chatId: new ChatId(id),
                        text: msg,
                        disableNotification: true
                        );
                else Bot.botClient.SendTextMessageAsync(
                    chatId: new ChatId(id),
                    text: msg
                    );
            }
            logger.Info($"Message {msg} was successfully sent to everyone.");
        }

        public static async void SendPossibleGroups(Telegram.Bot.Types.Chat chat)
        {
            await Bot.botClient.SendTextMessageAsync(
                chatId: chat,
                text: "Choose yours group",
                replyMarkup: new ReplyKeyboardMarkup
                {
                    Keyboard = new KeyboardButton[][]
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton{Text=Program.Groups[0] },
                            new KeyboardButton{Text=Program.Groups[1] },
                            new KeyboardButton{Text=Program.Groups[2] },
                            new KeyboardButton{Text=Program.Groups[3] }
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton{Text=Program.Groups[4] },
                            new KeyboardButton{Text=Program.Groups[5] },
                            new KeyboardButton{Text=Program.Groups[6] },
                            new KeyboardButton{Text=Program.Groups[7] },
                            new KeyboardButton{Text=Program.Groups[8] }
                        }
                    },
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                });
            logger.Info($"Possible groups was sent to {chat.Id}");
        }

        public static async void SendInstruction(Telegram.Bot.Types.Chat chat)
        {
            await Bot.botClient.SendTextMessageAsync(chatId: chat,
                text: "<b>Instruction</b>\n\n" +
                "Here is an article on usage:\n" +
                "<a href='https://telegra.ph/Instruction-04-04'>instruction</a> \n\n" +
                "Supported commands:\n" +
                "<i>/start</i> - if you made a mistake when \n" +
                "registering or <b>FATAL</b> error occured\n" +
                "<i>/help</i> - sends an instruction\n" +
                "<i>/send_again</i> - if inline keyboard\n" +
                "doesn't work\n\n" +
                "In any situation - <a href='https://t.me/ignatella'>support</a>",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableWebPagePreview: true
                );
        }
    }
}
