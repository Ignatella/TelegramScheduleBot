using Bot.Plots.Items;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace Bot
{
    class Program
    {
        public static readonly string BotToken;
        public static readonly string DBConnectionString;
        public static readonly string[] WorkingDays;
        public static string[] Groups { get; set; }

        public static string todayDayName;

        private static EventWaitHandle ewh;

        public static System.Timers.Timer timer = new System.Timers.Timer();

        static void Main(string[] args)
        {
            todayDayName = WorkingDays.Contains(DateTime.Now.DayOfWeek.ToString()) ?
                DateTime.Now.DayOfWeek.ToString() : WorkingDays[0];


            timer.Elapsed += Timer_Elapsed;
            timer.Interval = (TimeSpan.Zero.Subtract(DateTime.Now.TimeOfDay) + new TimeSpan(1, 0, 0, 0)).TotalMilliseconds;
            timer.Start();

            Bot bot = new Bot();
            try
            {
                Messages.SendСustomMessage("Bot is On, press /send_again button to get keyboard :)");
                bot.StartBot();
            }
            finally
            {
                Messages.SendСustomMessage("Bot is off, sorry.");
            }
        }
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Interval = new TimeSpan(1, 0, 0, 0).TotalMilliseconds;

            todayDayName = WorkingDays.Contains(DateTime.Now.DayOfWeek.ToString()) ?
                DateTime.Now.DayOfWeek.ToString() : WorkingDays[0];

            ewh = new EventWaitHandle(false, EventResetMode.AutoReset);

            Thread th = new Thread(g =>
            {

                Parallel.For(0, Bot.Keyboards.Keys.Count(), parallelOptions:
                    new ParallelOptions { MaxDegreeOfParallelism = 2 }, k => {

                        if (Bot.Keyboards.ElementAt(k).Value.inlineKeyboardForSchedule != null)
                        {
                            int MessageID = Bot.Keyboards.ElementAt(k).Value.inlineKeyboardForSchedule.MessageID;

                            Bot.Keyboards.ElementAt(k).Value.inlineKeyboardForSchedule =
                                GettingKeyboardWithRelevantLessons.
                                GetKeyboardWithRelevantLessonsForAutomaticRefresh(
                                    Bot.Keyboards.ElementAt(k).Key.ToString());

                            Bot.Keyboards.ElementAt(k).Value.inlineKeyboardForSchedule.MessageID = MessageID;
                        }
                    });

                ewh.Set();
            });

            th.Start();
            ewh.WaitOne();
        }
        static Program()
        {
            BotToken = XDocument.Load(Path.Combine(Environment.CurrentDirectory, "data.xml"))
               .Element("data").Element("BotToken").Value;

            DBConnectionString = XDocument.Load(Path.Combine(Environment.CurrentDirectory, "data.xml"))
               .Element("data").Element("DBConnString").Value;

            WorkingDays = (from day in XDocument.Load(Path.Combine(Environment.CurrentDirectory, "data.xml"))
                .Element("data").Element("day-array").Elements("item")
                           select day.Value).ToArray();

            Groups = (from day in XDocument.Load(Path.Combine(Environment.CurrentDirectory, "data.xml"))
               .Element("data").Element("group-array").Elements("item")
                      select day.Value).ToArray();

        }
    }
}
