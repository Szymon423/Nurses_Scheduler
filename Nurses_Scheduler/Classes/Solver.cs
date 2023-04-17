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

        // "Pielęgniarka", "Opiekun Medyczny", "Salowa", "Sanitariuszka", "Asystentka Pielęgniarki"
        // "Sekretarka", "Terapeuta zajęciowy", "Oddziałowa", 

        //  |--> first byte:
        //  |     |--> bit 0 is employy working on day shift
        //  |     |--> bit 1 is employy working on night shift
        //  |     |--> bit 2 is this a short shift
        //  |     |--> bit 3 is this some king of vacation - non working shitf
        //  |     |--> bit 4
        //  |     |--> bit 5
        //  |     |--> bit 6
        //  |     |--> bit 7 Error
        //  |--> second byte - error code

        public BitArray Data;
        public byte Error;


        public shiftData(bool dayShift = false, bool nightShift = false, bool shortShift = false, bool vacationDay = false)
        {
            Data = new BitArray(8, false);
            Data[0] = dayShift;
            Data[1] = nightShift;
            Data[2] = shortShift;
            Data[3] = vacationDay;
            // Data[4] = ...;
            // Data[5] = ...;
            // Data[6] = ...;
            Data[7] = true; // starting with error corresponging to wrong occupation
            Error = 0x01; 
        }


    }

    public class Solver
    {
        private List<EmployeeWorkArrangement> ewa;
        private List<int> eventDays;
        private int daysInMonth;
        private int employeeCount;
        private shiftData[/* employeeCount */, /* daysInMonth */] monthSchedule;
        private List<Employee> employeeList;


        public Solver (List<EmployeeWorkArrangement> _ewa, List<int> _eventDays, int _daysInMonth) 
        {
            ewa = _ewa;
            eventDays = _eventDays;
            daysInMonth = _daysInMonth;
            employeeCount = ewa.Count;
            monthSchedule = new shiftData[employeeCount, daysInMonth];
            employeeList = new List<Employee>();

            // asigninging proper shift data for all employees
            for (int employeeNumber = 0; employeeNumber < employeeCount; employeeNumber++)
            {
                employeeList.Add(ewa[employeeNumber].employee);
                List<string> temporaryEmployeeScheduleData = new List<string>(ewa[employeeNumber].GetWorkArrangementAsList());
                for (int day = 0; day < daysInMonth; day++)
                {                   
                    bool dayShift = temporaryEmployeeScheduleData[day].Equals("D") | temporaryEmployeeScheduleData[day].Equals("d");
                    bool nightShift = temporaryEmployeeScheduleData[day].Equals("N");
                    bool shortShift = temporaryEmployeeScheduleData[day].Equals("d");
                    bool vacationDay = false;
                    if (!(dayShift || nightShift || shortShift))
                    {
                        vacationDay = temporaryEmployeeScheduleData[day].Equals("U")  |
                                      temporaryEmployeeScheduleData[day].Equals("Um") |
                                      temporaryEmployeeScheduleData[day].Equals("Us") |
                                      temporaryEmployeeScheduleData[day].Equals("Uo") |
                                      temporaryEmployeeScheduleData[day].Equals("Uż") |
                                      temporaryEmployeeScheduleData[day].Equals("C")  |
                                      temporaryEmployeeScheduleData[day].Equals("Op");
                    }

                    monthSchedule[employeeNumber, day] = new shiftData(dayShift, nightShift, shortShift, vacationDay);
                }
            }
        }


        /// <summary>
        /// Function responsible for generating schedule which takes into consideration all hard constrains
        /// Those constrains are:
        ///     - ...
        /// </summary>
        private void GenerateHardCoistrainsCorrectSchedule()
        {

        }

        /// <summary>
        /// Function responsible ammending allready existing schedule according to employees wishes
        /// </summary>
        private void AmmendScheduleAccordingToSoftConstrains()
        {

        }

    }
}
