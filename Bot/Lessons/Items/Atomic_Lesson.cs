using Bot.Lessons.Items;
using System;

namespace Bot.Lessons.Items
{
    public struct Atomic_Lesson : IComparable<Atomic_Lesson>
    {
        private TimeSpan remainedTime;
        public TimeSpan RemainedTime
        {
            get
            {
                remainedTime = Starts.Subtract(DateTime.Now.TimeOfDay);
                if (remainedTime < TimeSpan.Zero)
                    remainedTime = TimeSpan.Zero;

                return remainedTime;
            }
        }
        public string LessonName { get; private set; }
        public string DayOfWeek { get; private set; }
        public int LessonID{ get; set; } 
        public TimeSpan Starts { get; private set; }
        public TimeSpan Ends { get; private set; }
        public string Auditorium { get; private set; }
        public Atomic_Lesson(string LessonName, string DayOfWeek,
                 string Auditorium, TimeSpan Starts, TimeSpan Ends)
        {
            this.LessonName = LessonName;
            this.DayOfWeek = DayOfWeek;
            this.Starts = Starts;
            this.Ends = Ends;
            this.Auditorium = Auditorium;
            LessonID = -1;
            remainedTime = Starts.Subtract(DateTime.Now.TimeOfDay);
        }
        public int CompareTo(Atomic_Lesson other)
        {
            return this.Starts.CompareTo(other.Starts);
        }

        public override string ToString()
        {
            return $"[RemainedTime: {RemainedTime}; LessonName: {LessonName}; DayOfWeek: {DayOfWeek};" +
                $"LessonID: {LessonID}; Starts: {Starts}; Ends: {Ends}; Auditorium: {Auditorium}]";
        }
    }
}