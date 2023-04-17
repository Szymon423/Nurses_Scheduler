using Nurses_Scheduler.Classes.DataBaseClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.RaportClases
{
    public class RaportData
    {
        public DepartmentWorkArrangement departmentWorkArrangement;
        public int monthLength;
        public int month;
        public int year;
        public Department department;
        public double monthlyHours;
        public List<int> eventDays;
        public string workingHoures;
        public RaportData(DepartmentWorkArrangement dwa, int monthLength, int month, int year, Department department, double monthlyHours, List<int> eventDays)
        {
            this.departmentWorkArrangement = dwa;
            this.monthLength = monthLength;
            this.month = month;
            this.year = year;   
            this.department = department;   
            this.monthlyHours = monthlyHours;
            this.eventDays = eventDays;
            this.workingHoures = convertWorkingHoures(this.monthlyHours);
        }

        public static string convertWorkingHoures(double monthlyHours)
        {
            int houres = (int)monthlyHours;
            int minutes = (int)Math.Round((monthlyHours - houres) * 60);
            int fullTimeDays = houres / 12;
            int shortShitfHoures = houres - fullTimeDays * 12;
            string test = houres.ToString() + "g " + minutes.ToString() + "min (" + fullTimeDays.ToString() + " x 12g + 1 x " + shortShitfHoures.ToString() + "g " + minutes.ToString() + "min)";
            Debug.WriteLine(test);
            return test;
        }
    }
}
