﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.DataBaseClasses
{
    [Table("EventDay")]
    public class EventDay
    {
        [PrimaryKey, AutoIncrement] 
        public int Id { get; set; }

        public int Day { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public string Description { get; set; } 

        public bool Repeatability { get; set; }

        [Ignore]
        public string DayInfo
        {
            get
            {
                if (Repeatability)
                {
                    return Day.ToString() + " " + App.months[Month - 1] + " Coroczne";
                }
                return Day.ToString() + " " + App.months[Month - 1] + " " + Year.ToString();
            }
        }

        public static List<EventDay> GetEventDaysFromDB()
        {
            List<EventDay> eventDays = new List<EventDay>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<EventDay>();
                eventDays = conn.Table<EventDay>().ToList();
            }
            return eventDays;
        }

        public static List<int> GetEventDaysFromDBasDaysNumbers(int month, int year)
        {
            List<EventDay> eventDays = new List<EventDay>();
            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<EventDay>();
                eventDays = conn.Table<EventDay>().ToList().Where(c => c.Month.Equals(month)).ToList();
            }

            List<int> days = new List<int>();
            foreach (EventDay day in eventDays) 
            {
                if (day.Repeatability == true || day.Year == year)
                {
                    days.Add(day.Day);
                }
            }
            return days;
        }
    }
}
