using Nurses_Scheduler.Classes.DataBaseClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes
{
    public struct shiftData
    {

        //       "Pielęgniarka", "Opiekun Medyczny", "Salowa", "Sanitariuszka", "Asystentka Pielęgniarki"
        //       "Sekretarka", "Terapeuta zajęciowy", "Oddziałowa", 

        //        |--> first byte:
        //        |     |--> bit 0 is employy working on day shift
        //        |     |--> bit 1 is employy working on night shift
        //        |     |--> bit 2 is this a short shift
        //        |     |--> bit 3 is this some king of vacation - non working shitf
        //        |     |--> bit 4
        //        |     |--> bit 5
        //        |     |--> bit 6
        //        |     |--> bit 7 Error
        //        |--> second byte - error code

        public BitArray Data;
        public byte Error;
        private int employeeId;
        private int occupation;
        // orrder as shown in App.AllowedOcupattions
        //  0 - Pielęgniarka
        //  1 - Opiekun Medyczny
        //  2 - Salowa
        //  3 - Sanitariuszka
        //  4 - Asystentka Pielęgniarki



        public shiftData(Employee employee)
        {
            Data = new BitArray(8, false);
            Data[7] = true; // starting with error corresponging to wrong occupation
            Error = 0x01; 
            employeeId = employee.Id;
            occupation = int.MaxValue; // max value meaning not found
            for (int i = 0; i < App.AllowedOccupations.Length; i++)
            {
                if (employee.Occupation == App.AllowedOccupations[i])
                {
                    occupation = i;
                    break;
                }
            } 
        }


    }

    public class Solver
    {
        private List<Employee> employeeList;
        private List<int> eventDays;
        private int daysInMonth;
        private shiftData[/* All_workers */, /* days_in_month */] monthSchedule;







        public Solver (List<Employee> _employeeList, List<int> _eventDays, int _daysInMonth) 
        {       
            employeeList = _employeeList;
            eventDays = _eventDays;
            daysInMonth = _daysInMonth;
            monthSchedule = new shiftData[employeeList.Count, daysInMonth];

        }

    }
}
