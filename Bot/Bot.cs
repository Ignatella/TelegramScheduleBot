using Bot.Plots.Items;
using Bot.DB.Items;
using Bot.InlineKeyboards;
using Bot.Plots;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    public class Bot
    {
        public static readonly ITelegramBotClient botClient =
            new TelegramBotClient(Program.BotToken);
        public static Dictionary<int, UsersKeyboards> Keyboards { get; set; } = new Dictionary<int, UsersKeyboards>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
        public void StartBot()
        {
            botClient.OnMessage += Bot_OnMessage;
            botClient.OnCallbackQuery += Bot_OnCallbackQuery;

            botClient.StartReceiving();

            Console.ReadLine();
        }
        private void Bot_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            if (!Keyboards.ContainsKey(e.CallbackQuery.From.Id))
                Keyboards[e.CallbackQuery.From.Id] = new UsersKeyboards();

            if (e.CallbackQuery.Data == "_")
            {
                botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                return;
            }

            if (Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForRegistration != null &&
                Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForRegistration.MessageID == e.CallbackQuery.Message.MessageId)
            {
                logger.Info("User wants to switch to usuall keyboard for schedule.");

                Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForRegistration.UpdateKeybordChoosen(e.CallbackQuery.Data);
                botClient.EditMessageReplyMarkupAsync(
                    chatId: e.CallbackQuery.Message.Chat,
                    messageId: e.CallbackQuery.Message.MessageId,
                    replyMarkup: Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForRegistration.Keyboard
                    );
                botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                return;
            }

            if(e.CallbackQuery.Data == "NextDay" && Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule != null)
            {
                UsersInfoForSelectsAndInserts person = new UsersInfoForSelectsAndInserts(
                chat_id: e.CallbackQuery.Message.Chat.Id.ToString());

                string lessons = Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule.NextDay(person);

                #region EditingMessageWithAnotherDayLessons
                if (Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule.NextDayMessageID != 0)
                {
                    botClient.EditMessageTextAsync(
                        chatId: e.CallbackQuery.Message.Chat,
                        messageId: Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule.NextDayMessageID,
                        text: lessons,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                        );
                }
                else
                {
                    Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule.NextDayMessageID
                       = botClient.SendTextMessageAsync(
                           chatId: e.CallbackQuery.Message.Chat,
                           text: lessons,
                           parseMode: Telegram.Bot.Types.Enums.ParseMode.Html).Result.MessageId;
                }

                #endregion

                botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
            }

            if (Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule is InlineKeyboardForSchedule &&
                Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule.MessageID == e.CallbackQuery.Message.MessageId)
            {
             
                var keyboard = (InlineKeyboardForSchedule)Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule;

                switch (e.CallbackQuery.Data)
                {
                    case "Refresh":
                        logger.Info("User pressed Refresh button.");

                        keyboard.Refresh();

                        #region DeleteTextMessageWithAnotherDayLessons
                        botClient.DeleteMessageAsync(
                            chatId: e.CallbackQuery.Message.Chat,
                            messageId: Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule.NextDayMessageID);
                        Keyboards[e.CallbackQuery.From.Id].inlineKeyboardForSchedule.NextDayMessageID = 0;

                        #endregion

                        botClient.EditMessageReplyMarkupAsync(
                            chatId: e.CallbackQuery.Message.Chat,
                            messageId: e.CallbackQuery.Message.MessageId,
                            replyMarkup: keyboard.Keyboard
                            );
                        break;
                    case "NextLesson":
                        logger.Info("User pressed NextLesson button.");

                        keyboard.NextLesson();

                        botClient.EditMessageReplyMarkupAsync(
                           chatId: e.CallbackQuery.Message.Chat,
                           messageId: e.CallbackQuery.Message.MessageId,
                           replyMarkup: keyboard.Keyboard
                           );
                        break;
                }
                botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                return;
            }
        }
        private async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (!Keyboards.ContainsKey(e.Message.From.Id))
                Keyboards[e.Message.From.Id] = new UsersKeyboards();

            if (e.Message.Text == "Send Again" || e.Message.Text == "/send_again")
            {
                logger.Info("User typed Send Again button");

                var keyboard = GettingKeyboardWithRelevantLessons.GetKeyboardWithRelevantLessons(e, true);
                if (keyboard != null)
                {
                    if (keyboard is InlineKeyboardForSchedule)
                        Keyboards[e.Message.From.Id].inlineKeyboardForSchedule = (InlineKeyboardForSchedule)keyboard;
                    else
                        Keyboards[e.Message.From.Id].inlineKeyboardForSchedule = keyboard;
                    return;
                }
            }

            #region RedistrationAndRessetingAccount
            if (e.Message.Text == "/start")
            {
                logger.Info("User typed Send /start button");

                Messages.SendPossibleGroups(e.Message.Chat);
                Keyboards[e.Message.From.Id] = new UsersKeyboards();

                Keyboards[e.Message.From.Id].startitngRegistration_plot = new StartitngRegistration();

                
                Messages.SendInstruction(e.Message.Chat);

                return;
            }

            if (Program.Groups.Contains(e.Message.Text) && 
                Keyboards[e.Message.From.Id].startitngRegistration_plot != null)
            {
                Keyboards[e.Message.From.Id].inlineKeyboardForRegistration =
                    Keyboards[e.Message.From.Id].startitngRegistration_plot.UserSelectedGroup(e);

                Keyboards[e.Message.From.Id].startitngRegistration_plot = null;
                return;
            }

            if (Keyboards[e.Message.From.Id].inlineKeyboardForRegistration != null && e.Message.Text == "READY")
            {
                logger.Info("User typed READY text."); 
                if (Keyboards[e.Message.From.Id].inlineKeyboardForRegistration.Name_Of_Choosen_Objects.Count() != 0)
                {

                    var keyboard = GettingKeyboardWithRelevantLessons.EndRegistrationGetKeyboardWithRelevantLessons(e,
                        Keyboards[e.Message.From.Id].inlineKeyboardForRegistration.Name_Of_Choosen_Objects.ToArray());

                    if (keyboard is InlineKeyboardForSchedule)
                        Keyboards[e.Message.From.Id].inlineKeyboardForSchedule = (InlineKeyboardForSchedule)keyboard;
                    else
                        Keyboards[e.Message.From.Id].inlineKeyboardForSchedule = keyboard;
                 
                    Keyboards[e.Message.From.Id].inlineKeyboardForRegistration = null;

                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: "Or Press /send_again button if keyboard doesn't work ",
                        replyMarkup: new ReplyKeyboardMarkup
                        {
                            Keyboard = new KeyboardButton[][]
                            {
                                new KeyboardButton[]
                                {
                                    new KeyboardButton{Text="Send Again"}
                                }
                            },
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        });

                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: "You didn't choose anything. Please choose!");
                }
            }

            #endregion

            if (e.Message.Text == "/help")
            {
                logger.Info("User typed /help text.");
                Messages.SendInstruction(e.Message.Chat);
                return;
            }

            if(e.Message.Text.Contains("/sndmsg"))
            {
                logger.Info($"User typed /sndmsg {e.Message.From.FirstName} {e.Message.From.LastName}");

                Thread th = new Thread(g =>
                {
                    if (e.Message.Text.Contains("-q"))
                        Messages.SendСustomMessage(string.Join(' ', e.Message.Text.Split(' ').Skip(2).ToArray()), true,
                            (true, e.Message.Chat));
                    else Messages.SendСustomMessage(string.Join(' ', e.Message.Text.Split(' ').Skip(1).ToArray()), false,
                           (true, e.Message.Chat));
                });
                th.IsBackground = true;
                th.Start();
                return;
            }
        }
    }
}
