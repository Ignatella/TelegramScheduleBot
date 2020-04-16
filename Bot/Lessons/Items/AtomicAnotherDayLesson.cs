using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Bot.Lessons.Items
{
    public struct AtomicAnotherDayLesson : IComparable<AtomicAnotherDayLesson>
    {
        public string LessonName { get; }
        public TimeSpan Starts { get; }
        public TimeSpan Stops { get; }

        public AtomicAnotherDayLesson(string LessonName, TimeSpan Starts, TimeSpan Stops )
        {
            this.LessonName = LessonName;
            this.Starts = Starts;
            this.Stops = Stops;
        }

        public int CompareTo(AtomicAnotherDayLesson other)
        {
            return this.Starts.CompareTo(other.Starts);
        }
    }
}