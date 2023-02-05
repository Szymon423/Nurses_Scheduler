using Nurses_Scheduler.Classes.DataBaseClasses;
using System;
using System.Collections.Generic;
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

        public RaportData(DepartmentWorkArrangement dwa, int monthLength, int month, int year, Department department, double monthlyHours)
        {
            this.departmentWorkArrangement = dwa;
            this.monthLength = monthLength;
            this.month = month;
            this.year = year;   
            this.department = department;   
            this.monthlyHours = monthlyHours;
        }
    }
}
