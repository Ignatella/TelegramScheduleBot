using Bot.Lessons.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Lessons
{
    public class AnotherDayLessons
    {
        #region Properties
        private AtomicAnotherDayLesson[] Lessons { get; set; }

        public string DayOfWeek { get; private set; }

        #endregion

        #region Constructors
        public AnotherDayLessons(string DayOfWeek, params AtomicAnotherDayLesson[] lessons) 
        {
            this.Lessons = lessons;
            Array.Sort(this.Lessons);
            this.DayOfWeek = DayOfWeek;
        }

        #endregion

        #region methods
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DayOfWeek.Substring(0, 3).ToUpper()}    Totally: {Lessons.Length} lesson(s)");
        
            for (int i = 0; i < Lessons.Length; i++)
                sb.AppendLine($"{i + 1}) <b>{Lessons[i].LessonName}</b> {Lessons[i].Starts} - {Lessons[i].Stops}");

            return sb.ToString();
        }

        #endregion
    }
}
