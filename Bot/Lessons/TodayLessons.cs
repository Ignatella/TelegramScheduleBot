using Bot.InlineKeyboards;
using Bot.Lessons.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Lessons
{
   
    public class TodayLessons
    {
        public event EventHandler<EventArgs> NoMoreLessonsTodayHander;

        #region Fields

        private LessonStatus currentStatus;
        private int nextLessonID;
        private int relevantLessonID;

        #endregion

        #region Properties
        public Atomic_Lesson RelevantLesson
        {
            get
            {
                return Info[RelevantLessonID];
            }
        }
        public Atomic_Lesson NextLesson
        {
            get
            {
                return Info[NextLessonID];
            }
        }
        public int NumOfLessonToday { get; set; }
        private Atomic_Lesson[] Info { get; set; }
        private LessonStatus CurrentStatus
        {
            get
            {
                return currentStatus;
            } 
            set
            {
                if (value == LessonStatus.NoMoreLessonsToday)
                {
                    NoMoreLessonsTodayHander?.Invoke(this, EventArgs.Empty);
                }
                currentStatus = value;
            }
        }
        private int NextLessonID
        {
            get
            {
                if (nextLessonID >= NumOfLessonToday - 1)
                    nextLessonID = -1;
                return ++nextLessonID;
            }
        }
        private int RelevantLessonID
        {
            get
            {
                for (int i = relevantLessonID; i < Info.Length; i++)
                {
                    if (Info[i].Starts > DateTime.Now.TimeOfDay)
                    {
                        relevantLessonID = i;
                        nextLessonID = relevantLessonID;
                        return relevantLessonID;
                    }
                }

                if (DateTime.Now.DayOfWeek.ToString() != Program.todayDayName)
                    return 0;

                CurrentStatus = LessonStatus.NoMoreLessonsToday;
                relevantLessonID = nextLessonID = NumOfLessonToday - 1;
                return relevantLessonID;
            }
        }

        #endregion

        #region Constructors
        public TodayLessons(params Atomic_Lesson[] info)
        {
            Info = info;
            Array.Sort(Info);
            for (int i = 0; i < Info.Length; i++) 
                Info[i].LessonID = i + 1;
            NumOfLessonToday = info.Length;
            CurrentStatus = LessonStatus.StillHaveLesson;
        }
        #endregion
    }
}