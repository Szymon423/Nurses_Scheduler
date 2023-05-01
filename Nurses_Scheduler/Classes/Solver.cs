using Nurses_Scheduler.Classes.DataBaseClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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


    public class shiftEmployees
    {
        public int employeeCount;
        public string occupation;

        public shiftEmployees(int _count, string _occupation)
        {
            employeeCount = _count;
            occupation = _occupation;
        }
    }

    public class dayEmployees
    {
        public List<shiftEmployees> employees_Day;
        public List<shiftEmployees> employees_Night;

        public dayEmployees()
        {
            employees_Day = new List<shiftEmployees>();
            employees_Night = new List<shiftEmployees>();
        }

        public dayEmployees(dayEmployees old)
        {
            employees_Day = new List<shiftEmployees>(old.employees_Day.Count);
            for (int i = 0; i < old.employees_Day.Count; i++)
            {
                employees_Day[i] = new shiftEmployees(old.employees_Day[i].employeeCount, old.employees_Day[i].occupation);
            }

            employees_Night = new List<shiftEmployees>(old.employees_Night.Count);
            for (int i = 0; i < old.employees_Night.Count; i++)
            {
                employees_Night[i] = new shiftEmployees(old.employees_Night[i].employeeCount, old.employees_Night[i].occupation);
            }
        }
    }

    /// <summary>
    /// Short def of this class
    /// </summary>
    public class Solver
    {
        private Department department;
        private List<EmployeeWorkArrangement> ewa;
        private List<int> eventDays;
        private int daysInMonth;
        private int employeeCount;
        private shiftData[/* employeeCount */, /* daysInMonth */] monthSchedule;
        private List<Employee> employeeList;
        private List<int> numberOfRecentlyWorkedSundays;
        private int requiredFullTimeShifts;
        private bool requiredPartTimeShift;
        private List<FundamentalEmployee> fundamentalEmployees_Day;
        private List<FundamentalEmployee> fundamentalEmployees_Night;
        private List<ComplementaryEmployee> complementaryEmployess_Day;
        private List<ComplementaryEmployee> complementaryEmployess_Night;
        private int followingWeeksInMonth;
        private bool[/* employeeCount */, /* weeksInMonth */] exisingBreakInFollowinng7DaysOfMonth;
        // const for entire month
        private dayEmployees expected_fundamentalEmplyees;
        // custom for each day
        private List<dayEmployees> my_fundamentalEmplyees;
        


        public Solver (DepartmentWorkArrangement dwa, List<int> _eventDays, int _daysInMonth, int _requiredFullTimeShifts, bool _requiredPartTimeShift) 
        {
            department = dwa.department;
            ewa = dwa.allEmployeeWorkArrangement;
            eventDays = _eventDays;
            daysInMonth = _daysInMonth;
            employeeCount = ewa.Count;
            monthSchedule = new shiftData[employeeCount, daysInMonth];
            employeeList = new List<Employee>();
            requiredFullTimeShifts = _requiredFullTimeShifts;
            requiredPartTimeShift = _requiredPartTimeShift;

            // asigninging proper shift data for all employees
            for (int employeeNumber = 0; employeeNumber < employeeCount; employeeNumber++)
            {
                employeeList.Add(ewa[employeeNumber].employee);
                List<string> temporaryEmployeeScheduleData = new List<string>(ewa[employeeNumber].GetWorkArrangementAsList());
                for (int day = 0; day < daysInMonth; day++)
                {
                    // bool dayShift = temporaryEmployeeScheduleData[day].Equals("D") | temporaryEmployeeScheduleData[day].Equals("d");
                    // bool nightShift = temporaryEmployeeScheduleData[day].Equals("N");
                    // bool shortShift = temporaryEmployeeScheduleData[day].Equals("d");
                    bool dayShift = false;
                    bool nightShift = false;
                    bool shortShift = false;
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

            // get data about all necessary emloyees types for given departement by shift type

            expected_fundamentalEmplyees = new dayEmployees();

            // fill all employees for day shifts
            foreach (FundamentalEmployee FE_D in FundamentalEmployee.GetFundamentalEmployeesDB(department.Id, "D"))
            {
                expected_fundamentalEmplyees.employees_Day.Add(new shiftEmployees(FE_D.MinEmployeesNumber, FE_D.Occupation));
            }

            // fill all employees for night shifts
            foreach (FundamentalEmployee FE_N in FundamentalEmployee.GetFundamentalEmployeesDB(department.Id, "N"))
            {
                expected_fundamentalEmplyees.employees_Night.Add(new shiftEmployees(FE_N.MinEmployeesNumber, FE_N.Occupation));
            }

            // prepare lists for each day in moonth for future complete
            my_fundamentalEmplyees = new List<dayEmployees>(daysInMonth);
            for (int i = 0; i < my_fundamentalEmplyees.Count; i++)
            {
                my_fundamentalEmplyees[i] = new dayEmployees(expected_fundamentalEmplyees);

                for (int j = 0; j < my_fundamentalEmplyees[i].employees_Day.Count; j++)
                {
                    my_fundamentalEmplyees[i].employees_Day[j].employeeCount = 0;
                }

                for (int j = 0; j < my_fundamentalEmplyees[i].employees_Night.Count; j++)
                {
                    my_fundamentalEmplyees[i].employees_Night[j].employeeCount = 0;
                }
            }

            complementaryEmployess_Day = ComplementaryEmployee.GetComplementaryEmployeesDB(department.Id, "D");
            complementaryEmployess_Night = ComplementaryEmployee.GetComplementaryEmployeesDB(department.Id, "N");

            // określ ile bieżących tygodni będzie w miesiącu
            if (daysInMonth == 28)
            {
                followingWeeksInMonth = 4;
            }
            else
            {
                followingWeeksInMonth = 5;
            }
            
            // by default all bools are false
            exisingBreakInFollowinng7DaysOfMonth = new bool[employeeCount, followingWeeksInMonth];

            // poprawić, tak by brało informacje z poprzednich grafików
            numberOfRecentlyWorkedSundays = new List<int>(employeeCount);
        }


        /// <summary>
        /// Function responsible for generating schedule which takes into consideration all hard constrains
        /// Those constrains are:
        ///     - ...
        /// </summary>
        private void GenerateHardCoistrainsCorrectSchedule()
        {
            for (int day = 0; day < daysInMonth; day++)
            {

                
            }
        }

        /// <summary>
        /// Function responsible ammending allready existing schedule according to employees wishes
        /// </summary>
        private void AmmendScheduleAccordingToSoftConstrains()
        {

        }

    }
}
