using System;
using System.Collections.Generic;
using System.Text;
using Bot.DB.Items;
using Bot.Lessons;
using Bot.Lessons.Items;
using MySql.Data.MySqlClient;
using NLog;

namespace Bot.DB
{
    public static class MySQL
    {
        private static string constr = Program.DBConnectionString; //Connection string
   
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MainPublicMethods
        public static string[] UserRegistrationOrUpdatingAndGettingNonRequiredLessons(UsersInfoForRegistration person)
        {
            List<string> text_for_buttons = new List<string>();

            var con = new MySqlConnection(constr);
            var cmd = new MySqlCommand();
            try
            {
                OpenAndConfigureConnection(ref con, ref cmd);
                    logger.Info(" Successfully opened and configured Connection in UserRegistration");
               
                BeginTransaction(cmd);
                    logger.Info("Successfully began the transaction in UserRegistration");


                if (CheckIfUserAlreadyRegistered(person, cmd))
                {
                    DeleteFromUsObjCommun(person, cmd);
                    logger.Info("Successfully deleted from USObjCommn in UpdatingGroupAndGettingNonRequiredLessons");

                    UpdateUsersGroup(person, cmd);
                    logger.Info("Successfully updated Users Group t in UpdatingGroupAndGettingNonRequiredLessons");
                }
                else
                {
                    InsertUserInfo(person, cmd);
                    logger.Info("Successfully inserted users info in UserRegistration");
                }

                InsertRequiredLessons(person, cmd);
                    logger.Info("Successfully inserted required lessons info in UserRegistration");

                text_for_buttons = 
                    SelectNonRequiredLessons(person, cmd);
                    logger.Info("Successfully selected  non required lessons info in UserRegistration");


                EndSuccessfulTransaction(cmd);
                    logger.Info("Transaction was successfully ended in UserRegistration");
            }
            finally
            {
                con.Close();
                cmd.Dispose();
            }

            return text_for_buttons.ToArray();
        }

        public static TodayLessons InsertingNonRequiredLessonsAndGettingTodaySchedule(string[] lessonList,
            UsersInfoForSelectsAndInserts person, string todayDayName)
        {
            TodayLessons todayLessons;

            var con = new MySqlConnection(constr);
            var cmd = new MySqlCommand();
            try
            {
                OpenAndConfigureConnection(ref con, ref cmd);
                    logger.Info("Successfully opened and configured Connection in InsertingNonRequiredLessonsAndGettingToday");

                BeginTransaction(cmd);
                    logger.Info("Successfully began the transaction in InsertingNonRequiredLessonsAndGettingToday");


                InsertNonRequiredLessons(lessonList, person, cmd);
                    logger.Info("Successfully inserted non required lessons info in InsertingNonRequiredLessonsAndGettingToday");

                todayLessons =
                    SelectTodaySchedule(person, todayDayName, cmd);
                    logger.Info("Successfully selected todays lessons  in InsertingNonRequiredLessonsAndGettingToday");

                EndSuccessfulTransaction(cmd);
                    logger.Info("Transaction was successfully ended in InsertingNonRequiredLessonsAndGettingToday");
            }
            finally
            {
                con.Close();
                cmd.Dispose();
            }

            return todayLessons;
        }

        public static TodayLessons GetTodaySchedule(UsersInfoForSelectsAndInserts person, string todayDayName,
            bool CheckIfUserExists = false)
        {
            TodayLessons todayLessons;

            var con = new MySqlConnection(constr);
            var cmd = new MySqlCommand();
            try
            {
                OpenAndConfigureConnection(ref con, ref cmd);
                    logger.Info("Successfully opened and configured Connection in GetTodaySchedule");

                BeginTransaction(cmd);
                    logger.Info("Successfully began the transaction in GetTodaySchedule");


                if (CheckIfUserExists)
                {
                    if (!CheckIfUserAlreadyRegistered(person, cmd))
                        return null;
                }

                todayLessons =
                     SelectTodaySchedule(person, todayDayName, cmd);
                    logger.Info("Successfully selected todays schedule in GetTodaySchedule");


                EndSuccessfulTransaction(cmd);
                    logger.Info("Transaction was successfully ended in GetTodaySchedule");
            }
            finally
            {
                con.Close();
                cmd.Dispose();
            }

            return todayLessons;
        }

        public static AnotherDayLessons GetAnotherDayLessons(UsersInfoForSelectsAndInserts person, string anotherDayName)
        {
            AnotherDayLessons anotherDayLessons;

            var con = new MySqlConnection(constr);
            var cmd = new MySqlCommand();
            try
            {
                OpenAndConfigureConnection(ref con, ref cmd);
                    logger.Info("Successfully opened and configured Connection in GetAnotherDayLessons");

                BeginTransaction(cmd);
                    logger.Info("Successfully began the transaction in GetAnotherDayLessons");


                anotherDayLessons =
                    SelectAnotherDaySchedule(anotherDayName, person, cmd);
                    logger.Info("Successfully selected anotherDaySchedule in GetAnotherDayLessons");


                EndSuccessfulTransaction(cmd);
                    logger.Info("Transaction was successfully ended in GetAnotherDayLessons");
            }
            finally
            {
                con.Close();
                cmd.Dispose();
            }

            return anotherDayLessons;
        }

        public static (bool, int[]) GetAllChatID((bool CheckIfUserIsAdmin, UsersInfoForSelectsAndInserts person) 
            dataForCheckingIfUserIsAdmin = default((bool, UsersInfoForSelectsAndInserts)))
        {
            int[] ids;
            bool isAdmin = false;

            var con = new MySqlConnection(constr);
            var cmd = new MySqlCommand();
            try
            {
                OpenAndConfigureConnection(ref con, ref cmd);
                logger.Info("Successfully opened and configured Connection in GetAnotherDayLessons");

                BeginTransaction(cmd);
                logger.Info("Successfully began the transaction in GetAnotherDayLessons");

                if (dataForCheckingIfUserIsAdmin.CheckIfUserIsAdmin)
                {
                    if (!IsUserAnAdmin(cmd, dataForCheckingIfUserIsAdmin.person))
                    {
                        logger.Info($"UserIsNotAnAdmin. User data = {dataForCheckingIfUserIsAdmin.person}");
                        return (false, null);
                    }
                }

                isAdmin = true;

                ids = SelectAllChatID(cmd);

                logger.Info("Successfully selected all chat IDs from table users");

                EndSuccessfulTransaction(cmd);
                logger.Info("Transaction was successfully ended in GetAnotherDayLessons");
            }
            finally
            {
                con.Close();
                cmd.Dispose();
            }

            return (isAdmin, ids);
        }

        #endregion

        #region PrivateSupportingMethods
        private static TodayLessons SelectTodaySchedule(UsersInfoForSelectsAndInserts person, 
            string todayDayName, MySqlCommand cmd)
        {
            List<Atomic_Lesson> lessons = new List<Atomic_Lesson>();
          
            string sql = "select obj_name, day_name, sala_name, starts, stops from schedule sch " +
            "join " +
                "obj_info oi on (sch.object_id = oi.object_id) " +
            "join " +
                "obj_name_list onl on (oi.obj_name_id = onl.obj_id) " +
            "join " +
                "days_of_week dow on (sch.day_id = dow.day_id) " +
            "join " +
                "salas s on (sch.sala_id = s.sala_id) " +
            "join " +
                "time t on (sch.time_id = t.time_id) " +
            "where day_name = @todayDayName " +
                "and " +
                    "oi.object_id in " +
                    "(select object_id from us_obj_commun where user_id = " +
                    "(select user_id from users where chat_id = @chat_id))";

            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@todayDayName"))
                cmd.Parameters.AddWithValue("@todayDayName", todayDayName);

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lessons.Add(new Atomic_Lesson(rdr[0].ToString(), rdr[1].ToString(),
                    rdr[2].ToString(), (TimeSpan)rdr[3], (TimeSpan)rdr[4]));
            }

            return new TodayLessons(lessons.ToArray());
            
        }

        private static AnotherDayLessons SelectAnotherDaySchedule(string anotherDayName,
          UsersInfoForSelectsAndInserts person, MySqlCommand cmd)
        {
            List<AtomicAnotherDayLesson> lessons = new List<AtomicAnotherDayLesson>();


            string sql = "select obj_name, starts, stops from schedule sch " +
            "join " +
                "obj_info oi on (sch.object_id = oi.object_id) " +
            "join " +
                "obj_name_list onl on (oi.obj_name_id = onl.obj_id) " +
            "join " +
                "days_of_week dow on (sch.day_id = dow.day_id) " +
            "join " +
                "time t on (sch.time_id = t.time_id) " +
            "where day_name = @AnotherDayName " +
                "and " +
                    "oi.object_id in " +
                    "(select object_id from us_obj_commun where user_id = " +
                    "(select user_id from users where chat_id = @chat_id))";


            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@AnotherDayName"))
                cmd.Parameters.AddWithValue("@AnotherDayName", anotherDayName);

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lessons.Add(new AtomicAnotherDayLesson(rdr[0].ToString(), (TimeSpan)rdr[1], (TimeSpan)rdr[2]));
            }

            return new AnotherDayLessons(anotherDayName, lessons.ToArray());
          
        }

        private static void InsertNonRequiredLessons(string[] lessonsList, 
            UsersInfoForSelectsAndInserts person, MySqlCommand cmd)
        {

            string sql = "insert into us_obj_commun (user_id, object_id) " +
                "select * from (select user_id from users where chat_id = @chat_id) a " +
                "join (select object_id from obj_info where object_id in " +
                    "(select object_id from group_obj_commun where group_id = " +
                        "(select group_id from users where chat_id = @chat_id) ) " +
                "and obj_name_id in " +
                    "(select obj_id from obj_name_list where obj_name in " +
                        "(" + lessonsList.ArrayToStringWichCommaForMySQL("lesson_name", cmd) + "))) b;";
            
            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
           
        }

        private static void InsertUserInfo(UsersInfoForRegistration person, MySqlCommand cmd)
        {
          
            string sql = "insert into users " +
                "(name, surname, username, chat_id, group_id) " +
                "values " +
                    "(@first_name, @last_name, @user_name, @chat_id, " +
                    "(select group_id from groups where group_name = @group_name))";

            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@first_name"))
                cmd.Parameters.AddWithValue("@first_name", person.first_name);

            if (!cmd.Parameters.Contains("@last_name"))
                cmd.Parameters.AddWithValue("@last_name", person.last_name);

            if (!cmd.Parameters.Contains("@user_name"))
                cmd.Parameters.AddWithValue("@user_name", person.user_name);

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            if (!cmd.Parameters.Contains("@group_name"))
                cmd.Parameters.AddWithValue("@group_name", person.group_name);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
            
        }

        private static void DeleteFromUsObjCommun(UsersInfoForSelectsAndInserts person, MySqlCommand cmd)
        {
           
            string sql = "delete from us_obj_commun " +
                "where " +
                    "user_id = (select user_id from users where chat_id = @chat_id)";

            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        private static bool CheckIfUserAlreadyRegistered(UsersInfoForSelectsAndInserts person, MySqlCommand cmd)
        {
            
            string sql = "select * from users where chat_id = @chat_id limit 1";

            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();

            using var rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                logger.Info($"Users already exists {person}");
                return true;
            }
            
            logger.Info($"No users found, data: {person}");
            return false;
        }

        private static int[] SelectAllChatID(MySqlCommand cmd)
        {
            List<int> ids = new List<int>();

            string sql = "select chat_id from users";

            cmd.CommandText = sql;

            cmd.Prepare();

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ids.Add(Convert.ToInt32(rdr[0]));
            }

            return ids.ToArray();
        }

        private static bool IsUserAnAdmin(MySqlCommand cmd, UsersInfoForSelectsAndInserts person)
        {
           
            string sql = "select * from users where chat_id = @chat_id and is_admin = 1";

            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();

            using var rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
                return true;

            return false;
        }

        #region AlwaysInLigaments 

        private static List<string> SelectNonRequiredLessons(UsersInfoForSelectsAndInserts person ,MySqlCommand cmd)
        {
            List<string> text_for_buttons = new List<string>();
           
            string sql = "select distinct obj_name from " +
                "(select * from obj_info where required = 0 and " +
                "object_id in " +
                "(select object_id from group_obj_commun where group_id = " +
                "(select group_id from users where chat_id = @chat_id))) a join " +
                "obj_name_list b on (a.obj_name_id = b.obj_id) order by obj_name ";

            cmd.CommandText = sql;
                
            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                text_for_buttons.Add(rdr[0].ToString());
            }
           
            return text_for_buttons;
        }
        private static void InsertRequiredLessons(UsersInfoForSelectsAndInserts person, MySqlCommand cmd)
        {
           
            //insert into us_obj_commun required lessons
            string sql = "insert into us_obj_commun (user_id, object_id) " +
                "select * from " +
                    "((select user_id from users where chat_id = @chat_id) us_id " +
                "join (select object_id from obj_info where required = 1 and object_id in " +
                    "(select distinct object_id from group_obj_commun where group_id = " +
                    "(select group_id from users where chat_id = @chat_id))) obj_id )";

            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
          
        }
        private static void UpdateUsersGroup(UsersInfoForResetting person ,MySqlCommand cmd)
        {
           
            string sql = "update users set group_id = " +
                "(select group_id from groups where group_name = @group_name) " +
                "where chat_id = @chat_id";

            cmd.CommandText = sql;

            if (!cmd.Parameters.Contains("@group_name"))
                cmd.Parameters.AddWithValue("@group_name", person.group_name);
               
            if (!cmd.Parameters.Contains("@chat_id"))
                cmd.Parameters.AddWithValue("@chat_id", person.chat_id);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

           
        }

        #endregion

        #region OpenAndConfigureConnection
        private static void OpenAndConfigureConnection(ref MySqlConnection con, ref MySqlCommand cmd)
        {
            con.Open();
            cmd.Connection = con;
            cmd.CommandTimeout = 3;
        }

        #endregion

        #region BeginAndEndTransaction
        private static void BeginTransaction(MySqlCommand cmd)
        {
            string sql = "BEGIN";
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static void EndSuccessfulTransaction(MySqlCommand cmd)
        {
            string sql = "COMMIT";
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
        #endregion

        #endregion
    }
}