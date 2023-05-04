using Nurses_Scheduler.Classes.DataBaseClasses;
using Nurses_Scheduler.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
                employees_Day.Add(new shiftEmployees(old.employees_Day[i].employeeCount, old.employees_Day[i].occupation));
            }

            employees_Night = new List<shiftEmployees>(old.employees_Night.Count);
            for (int i = 0; i < old.employees_Night.Count; i++)
            {
                employees_Night.Add(new shiftEmployees(old.employees_Night[i].employeeCount, old.employees_Night[i].occupation));
            }
        }
    }


    public class employeeGroupIndex
    {
        public int start;
        public int end;

        public employeeGroupIndex(int _start, int _end)
        {
            start = _start;
            end = _end;
        }

        public void print()
        {
            Debug.WriteLine("start: " + start.ToString() + " end: " + end.ToString());
        }
    }


    public class Solver
    {
        private Department department;
        private DepartmentWorkArrangement dwa;
        private DepartmentWorkArrangement modified_dwa;
        private DepartmentWorkArrangement prevoiusMonth_dwa;
        private List<EmployeeWorkArrangement> ewa;
        private List<int> eventDays;
        private int daysInMonth;
        private int year;
        private int month;
        private int employeeCount;
        private shiftData[/* employeeCount */, /* daysInMonth */] monthSchedule;
        private shiftData[/* employeeCount */] lastDayOfPreviousMonthSchedule;
        private List<Employee> employeeList;
        private List<int> numberOfRecentlyWorkedSundays;
        private int[] requiredFullTimeShifts;
        private bool[] requiredPartTimeShift;
        private List<FundamentalEmployee> fundamentalEmployees_Day;
        private List<FundamentalEmployee> fundamentalEmployees_Night;
        private List<string> fundamentalEmployessOccupations_Day;
        private List<string> fundamentalEmployessOccupations_Night;
        private List<ComplementaryEmployee> complementaryEmployess_Day;
        private List<ComplementaryEmployee> complementaryEmployess_Night;
        private List<string> complementaryEmployessOccupations_Day;
        private List<string> complementaryEmployessOccupations_Night;
        private int minimalEmployeeNumberPerDayShift;
        private int minimalEmployeeNumberPerNightShift;
        private int followingWeeksInMonth; 
        private bool[/* employeeCount */, /* weeksInMonth */] exisingBreakInFollowinng7DaysOfMonth;
        private dayEmployees expected_fundamentalEmplyees; // const for entire month  
        private List<dayEmployees> my_fundamentalEmplyees; // custom for each day
        private List<employeeGroupIndex> employeeGroupIndices;
        private IDictionary<string, int> OccupationToGroupIndex = new Dictionary<string, int>();
        private int[] ShiftCounter;
        private int[] employeesOnDayShiftCounter;
        private int[] employeesOnNightShiftCounter;
        private List<int> sundaysList;
        private List<int> sundaysListInPrevoiusMonth;
        private List<int> mondaysList;
        private List<(int _day, int _employeeCount)> queueOfDaysDay;
        private List<(int _day, int _employeeCount)> queueOfDaysNight;



        public  Solver (DepartmentWorkArrangement _dwa, List<int> _eventDays, int _year, int _month) 
        {
            dwa = _dwa;
            department = _dwa.department;
            ewa = _dwa.allEmployeeWorkArrangement;
            eventDays = _eventDays;
            year = _year;
            month = _month;
            daysInMonth = DateTime.DaysInMonth(_year, _month);
            employeeCount = ewa.Count;
            monthSchedule = new shiftData[employeeCount, daysInMonth];
            lastDayOfPreviousMonthSchedule = new shiftData[employeeCount];
            employeeList = new List<Employee>();
            requiredFullTimeShifts = new int[employeeCount];
            requiredPartTimeShift = new bool[employeeCount];
            minimalEmployeeNumberPerDayShift = department.MinimalEmployeePerDayShift;
            minimalEmployeeNumberPerNightShift = department.MinimalEmployeePerNightShift;
            employeeGroupIndices = new List<employeeGroupIndex>();
            employeeGroupIndex currentGroup;
            int start = 1;
            int end = 1;

            // asigninging proper shift data for all employees
            for (int employeeNumber = 0; employeeNumber < employeeCount; employeeNumber++)
            {
                if (employeeNumber != 0 && ewa[employeeNumber].employee == null)
                {
                    end = employeeNumber - 1;
                    currentGroup = new employeeGroupIndex(start, end);
                    employeeGroupIndices.Add(currentGroup);
                    OccupationToGroupIndex.Add(ewa[start].employee.Occupation, employeeGroupIndices.Count - 1);
                    start = employeeNumber + 1;
                } 
                
                
                employeeList.Add(ewa[employeeNumber].employee);
                lastDayOfPreviousMonthSchedule[employeeNumber] = new shiftData(false, false, false, false);

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
            end = employeeCount - 1;
            currentGroup = new employeeGroupIndex(start, end);
            employeeGroupIndices.Add(currentGroup);
            OccupationToGroupIndex.Add(ewa[start].employee.Occupation, employeeGroupIndices.Count - 1);

            foreach (var o in employeeGroupIndices)
            {
                o.print();
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
            for (int i = 0; i < daysInMonth; i++)
            {
                my_fundamentalEmplyees.Add(new dayEmployees(expected_fundamentalEmplyees));

                // setting total employees count assigned to this shift as 0
                for (int j = 0; j < my_fundamentalEmplyees[i].employees_Day.Count; j++)
                {
                    my_fundamentalEmplyees[i].employees_Day[j].employeeCount = 0;
                }

                // setting total employees count assigned to this shift as 0
                for (int j = 0; j < my_fundamentalEmplyees[i].employees_Night.Count; j++)
                {
                    my_fundamentalEmplyees[i].employees_Night[j].employeeCount = 0;
                }
            }

            complementaryEmployess_Day = ComplementaryEmployee.GetComplementaryEmployeesDB(department.Id, "D");
            complementaryEmployess_Night = ComplementaryEmployee.GetComplementaryEmployeesDB(department.Id, "N");

            complementaryEmployessOccupations_Day = new List<string>();
            complementaryEmployessOccupations_Night = new List<string>();

            foreach (ComplementaryEmployee cmpl in complementaryEmployess_Day)
            {
                complementaryEmployessOccupations_Day.Add(cmpl.Occupation);
            }

            foreach (ComplementaryEmployee cmpl in complementaryEmployess_Night)
            {
                complementaryEmployessOccupations_Night.Add(cmpl.Occupation);
            }

            fundamentalEmployessOccupations_Day = new List<string>();
            fundamentalEmployessOccupations_Night = new List<string>();

            foreach (shiftEmployees cmpl in expected_fundamentalEmplyees.employees_Day)
            {
                fundamentalEmployessOccupations_Day.Add(cmpl.occupation);
            }

            foreach (shiftEmployees cmpl in expected_fundamentalEmplyees.employees_Night)
            {
                fundamentalEmployessOccupations_Night.Add(cmpl.occupation);
            }



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

            // make it so it takes data from previous schedules
            numberOfRecentlyWorkedSundays = new List<int>(employeeCount);
            for (int i = 0; i < employeeCount; i++)
            {
                numberOfRecentlyWorkedSundays.Add(0);
            }

            modified_dwa = new DepartmentWorkArrangement(this.dwa);
            modified_dwa.CleanAllEmployeeWorkArrangement();

            // prepare shift counting array, foreach employee set it to 0
            ShiftCounter = new int[employeeCount];
            for (int i = 0; i < ShiftCounter.Length; i++)
            {
                ShiftCounter[i] = 0;
            }

            // make number of employees on shifts equal 0
            employeesOnDayShiftCounter = new int[daysInMonth];
            employeesOnNightShiftCounter = new int[daysInMonth];
            for (int i = 0; i < daysInMonth; i++)
            {
                employeesOnDayShiftCounter[i] = 0;
                employeesOnNightShiftCounter[i] = 0;
            }

            // find which days are sundays
            sundaysList = new List<int>();
            mondaysList = new List<int>();
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime dt = new DateTime(year, month, day);
                if (dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    sundaysList.Add(day);
                }
                if (dt.DayOfWeek == DayOfWeek.Monday)
                {
                    mondaysList.Add(day);
                }
            }

            queueOfDaysDay = new List<(int, int)>();
            queueOfDaysNight = new List<(int, int)>();

            CalculateEmployeesWorkingHoures();
            getDataFromPreviousMonth();
            GenerateHardCoistrainsCorrectSchedule();
        }


        private void GenerateHardCoistrainsCorrectSchedule()
        {
            Random rand = new Random();
            for (int day = 0; day < daysInMonth; day++)
            {
                bool isThisSunday = sundaysList.Contains(day + 1);
                bool isThisMonday = mondaysList.Contains(day + 1);

                // fill up schedule with fundamental employees on day shifts
                for (int gruop_id = 0; gruop_id < my_fundamentalEmplyees[day].employees_Night.Count; gruop_id++)
                {
                    // if on given day are missing complementary employees of type 0 (pielęgniarki)
                    while (my_fundamentalEmplyees[day].employees_Night[gruop_id].employeeCount < expected_fundamentalEmplyees.employees_Night[gruop_id].employeeCount)
                    {
                        // random chose number of employee froom group of 0 (pielęgniarki)
                        string occupation = expected_fundamentalEmplyees.employees_Night[gruop_id].occupation;
                        int employeeNumFromCurrentGroup = rand.Next(employeeGroupIndices[OccupationToGroupIndex[occupation]].start, employeeGroupIndices[OccupationToGroupIndex[occupation]].end + 1);
                        bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "N", day) &
                                          CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day) &
                                          CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup) &
                                          CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "N");

                        if (isThisSunday)
                        {
                            canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "N");
                        }
                        if (canProceed)
                        {
                            monthSchedule[employeeNumFromCurrentGroup, day].Data[1] = true;
                            ShiftCounter[employeeNumFromCurrentGroup] += 1;
                            my_fundamentalEmplyees[day].employees_Night[gruop_id].employeeCount += 1;
                            employeesOnNightShiftCounter[day] += 1;
                            Debug.WriteLine("occupation: " + occupation + " night: " + day.ToString() + " employee: " + employeeNumFromCurrentGroup.ToString());
                        }
                    }
                }

                // fill up schedule with fundamental employees on night shifts
                for (int gruop_id = 0; gruop_id < my_fundamentalEmplyees[day].employees_Day.Count; gruop_id++)
                {
                    // if on given day are missing complementary employees of type 0 (pielęgniarki)
                    while (my_fundamentalEmplyees[day].employees_Day[gruop_id].employeeCount < expected_fundamentalEmplyees.employees_Day[gruop_id].employeeCount)
                    {
                        // random chose number of employee froom group of 0 (pielęgniarki)
                        string occupation = expected_fundamentalEmplyees.employees_Day[gruop_id].occupation;
                        int employeeNumFromCurrentGroup = rand.Next(employeeGroupIndices[OccupationToGroupIndex[occupation]].start, employeeGroupIndices[OccupationToGroupIndex[occupation]].end + 1);
                        bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "D", day)      &
                                          CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day)   &
                                          CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup)         &
                                          CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "D");
                        
                        if (isThisSunday)
                        {
                            canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "D");
                        }
                        if (canProceed)
                        {
                            monthSchedule[employeeNumFromCurrentGroup, day].Data[0] = true;
                            ShiftCounter[employeeNumFromCurrentGroup] += 1;
                            my_fundamentalEmplyees[day].employees_Day[gruop_id].employeeCount += 1;
                            employeesOnDayShiftCounter[day] += 1;
                            Debug.WriteLine("occupation: " + occupation + " day: " + day.ToString() + " employee: " + employeeNumFromCurrentGroup.ToString());
                        }
                    }
                }
            }

            Debug.WriteLine("Pracownicy podstawowi uzupełnieni");

            for (int day = 0; day < daysInMonth; day++)
            {
                bool isThisSunday = sundaysList.Contains(day + 1);
                bool isThisMonday = mondaysList.Contains(day + 1);

                // fill up schedule with complementary employees on day shifts
                while (employeesOnDayShiftCounter[day] < minimalEmployeeNumberPerDayShift)
                {
                    string occupation = complementaryEmployessOccupations_Day[rand.Next(0, complementaryEmployessOccupations_Day.Count - 1)];
                    int employeeNumFromCurrentGroup = rand.Next(employeeGroupIndices[OccupationToGroupIndex[occupation]].start, employeeGroupIndices[OccupationToGroupIndex[occupation]].end + 1);

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "D", day) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day) &
                                      CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "D");
                    
                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "D");
                    }
                    if (canProceed)
                    {
                        monthSchedule[employeeNumFromCurrentGroup, day].Data[0] = true;
                        ShiftCounter[employeeNumFromCurrentGroup] += 1;
                        employeesOnDayShiftCounter[day] += 1;
                        Debug.WriteLine("Complementary: \t occupation: " + occupation + " day: " + day.ToString() + " employee: " + employeeNumFromCurrentGroup.ToString());
                    }
                }

                // fill up schedule with complementary employees on night shifts
                while (employeesOnNightShiftCounter[day] < minimalEmployeeNumberPerNightShift)
                {
                    string occupation = complementaryEmployessOccupations_Night[rand.Next(0, complementaryEmployessOccupations_Night.Count - 1)];
                    int employeeNumFromCurrentGroup = rand.Next(employeeGroupIndices[OccupationToGroupIndex[occupation]].start, employeeGroupIndices[OccupationToGroupIndex[occupation]].end + 1);

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "N", day) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day) &
                                      CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "N");

                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "N");
                    }
                    if (canProceed)
                    {
                        monthSchedule[employeeNumFromCurrentGroup, day].Data[1] = true;
                        ShiftCounter[employeeNumFromCurrentGroup] += 1;
                        employeesOnNightShiftCounter[day] += 1;
                        Debug.WriteLine("Complementary: \t occupation: " + occupation + " night: " + day.ToString() + " employee: " + employeeNumFromCurrentGroup.ToString());
                    }
                }
            }

            Debug.WriteLine("Uzupełniono do minimalnego wymaganego składu pracowników");

            for (int employeeNumber = 0; employeeNumber < employeeCount; employeeNumber++)
            {
                if (employeeList[employeeNumber] == null)
                {
                    continue;
                }

                while (ShiftCounter[employeeNumber] < requiredFullTimeShifts[employeeNumber])
                {
                    string shiftType;
                    // with probability of 66% assign day shift, with probability 34% assign night shift
                    if (complementaryEmployessOccupations_Night.Contains(employeeList[employeeNumber].Occupation) || fundamentalEmployessOccupations_Night.Contains(employeeList[employeeNumber].Occupation))
                    {
                        if (rand.Next(0, 101) > 90) // chosen night
                        {
                            shiftType = "N";
                        }
                        else // chosen day
                        {
                            shiftType = "D";
                        }
                    }
                    else // only day shift
                    {
                        shiftType = "D";
                    }

                    updateQueueWithDays();
                    // choose day with linear possibility - odwrotna dystrybuanta funkcja: f(x) = 1 - x
                    // day lower in List - bigger possibility of beeing choosen
                    double uniformPossibility = rand.Next(0, 5000) / 10000.0;
                    double linearPossibility = 1.0 - Math.Sqrt(1.0 - 2.0 * uniformPossibility);
                    int dayIndex = (int)(linearPossibility * daysInMonth);
                    int day;
                    if (shiftType.Equals("D"))
                    {
                        day = queueOfDaysDay[dayIndex]._day;
                    }
                    else
                    {
                        day = queueOfDaysNight[dayIndex]._day;
                    }                

                    bool isThisSunday = sundaysList.Contains(day + 1);

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumber, shiftType, day) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumber, day) &
                                      CheckIfAll_12h_ShiftsAreAssigned(employeeNumber) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumber, day, shiftType);

                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumber, day, shiftType);
                    }
                    if (canProceed)
                    {
                        if (shiftType.Equals("D"))
                        {
                            monthSchedule[employeeNumber, day].Data[0] = true;
                            employeesOnDayShiftCounter[day] += 1;
                        }
                        else if (shiftType.Equals("N"))
                        {
                            monthSchedule[employeeNumber, day].Data[1] = true;
                            employeesOnNightShiftCounter[day] += 1;
                        }
                        ShiftCounter[employeeNumber] += 1; 
                        Debug.WriteLine("Complementary: \t occupation: " + employeeList[employeeNumber].Occupation + " " + shiftType + ": " + day.ToString() + " employee: " + employeeNumber.ToString());
                    }
                }
            }

            Debug.WriteLine("Uzupełniono pełne zmiany wszystkim pracownikom");

            for (int employeeNumber = 0; employeeNumber < employeeCount; employeeNumber++)
            {
                if (employeeList[employeeNumber] == null)
                {
                    continue;
                }

                while (requiredPartTimeShift[employeeNumber])
                {
                    // choose day
                    int day = rand.Next(0, daysInMonth);
                    bool isThisSunday = sundaysList.Contains(day + 1);
                    string shiftType = "d";

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumber, shiftType, day) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumber, day) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumber, day, shiftType);

                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumber, day, shiftType);
                    }
                    if (canProceed)
                    {
                        monthSchedule[employeeNumber, day].Data[0] = true;
                        monthSchedule[employeeNumber, day].Data[2] = true;
                        requiredPartTimeShift[employeeNumber] = false;
                        employeesOnNightShiftCounter[day] += 1;
                        Debug.WriteLine("ShortShift: \t occupation: " + employeeList[employeeNumber].Occupation + " " + shiftType + ": " + day.ToString() + " employee: " + employeeNumber.ToString());
                    }
                }
            }

            Debug.WriteLine("Uzupełniono krótką zmianę wszystkim pracownikom");

            Debug.WriteLine("statystyki zmiany dziennej");
            foreach ((int _day, int _employeeCount) data in queueOfDaysDay)
            {
                Debug.WriteLine("day: " + data._day.ToString() + "\t Count: " + data._employeeCount.ToString());
            }

            Debug.WriteLine("statystyki zmiany nocnej");
            foreach ((int _day, int _employeeCount) data in queueOfDaysNight)
            {
                Debug.WriteLine("day: " + data._day.ToString() + "\t Count: " + data._employeeCount.ToString());
            }
        }


        private void AmmendScheduleAccordingToSoftConstrains()
        {

        }


        private bool CheckIfNotFourthSunday(int employeeIndex , int day, string shiftToInsert)
        {
            // copy of previous state
            BitArray copy = new BitArray(monthSchedule[employeeIndex, day].Data);

            // temporary insert this shift
            if (shiftToInsert.Equals("D"))
            {
                monthSchedule[employeeIndex, day].Data[0] = true;
            }
            else if (shiftToInsert.Equals("N"))
            {
                monthSchedule[employeeIndex, day].Data[1] = true;
            }
            else if (shiftToInsert.Equals("d"))
            {
                monthSchedule[employeeIndex, day].Data[0] = true;
                monthSchedule[employeeIndex, day].Data[1] = true;
            }

            bool canProceed = true;

            int temporaryCounter = numberOfRecentlyWorkedSundays[employeeIndex];

            foreach (int sunday_i in sundaysList)
            {
                if (monthSchedule[employeeIndex, sunday_i - 1].Data[0] == true || monthSchedule[employeeIndex, sunday_i - 1].Data[1] == true)
                {
                    temporaryCounter += 1;

                    if (temporaryCounter > 3)
                    {
                        canProceed = false;
                        break;
                    }
                }
                else
                {
                    temporaryCounter = 0;
                }
            }
  
            // rollback previous temporary changes
            monthSchedule[employeeIndex, day].Data = copy;

            return canProceed;
        }


        private bool CheckIf_12h_BetweenShifts(int EmployeeIndex, string ShiftType, int day)
        {
            if (day > 0)
            {
                // check on current month data
                if (ShiftType.Equals("D"))
                {
                    bool nightBefore = this.monthSchedule[EmployeeIndex, day - 1].Data[1];
                    return !nightBefore;
                }
                if (ShiftType.Equals("N"))
                {
                    bool currentDay = this.monthSchedule[EmployeeIndex, day].Data[0];
                    bool daytAfter = false;
                    if (day < daysInMonth - 1)
                    {
                        daytAfter = this.monthSchedule[EmployeeIndex, day + 1].Data[0];
                    }  
                    return !currentDay & !daytAfter;
                }
                if (ShiftType.Equals("d"))
                {
                    bool nightBefore = this.monthSchedule[EmployeeIndex, day - 1].Data[1];
                    return !nightBefore;
                }
            }
            else
            {
                // check on previous month data
                if (ShiftType.Equals("D"))
                {
                    bool nightBefore = this.lastDayOfPreviousMonthSchedule[EmployeeIndex].Data[1];
                    return !nightBefore;
                }
                if (ShiftType.Equals("N"))
                {
                    bool currentDay = this.lastDayOfPreviousMonthSchedule[EmployeeIndex].Data[0];
                    bool daytAfter = this.monthSchedule[EmployeeIndex, 0].Data[0];
                    return !currentDay & !daytAfter;
                }
                if (ShiftType.Equals("d"))
                {
                    bool nightBefore = this.lastDayOfPreviousMonthSchedule[EmployeeIndex].Data[1];
                    return !nightBefore;
                }
            }
            Debug.WriteLine("12h between shifts returned false");
            return false;
        }


        private bool CheckIfExistAllreadyAssignedShift(int EmployeeIndex, int day)
        {
            bool currentDay = monthSchedule[EmployeeIndex, day].Data[0];
            bool currentNight = monthSchedule[EmployeeIndex, day].Data[1];   
            return !(currentDay | currentNight);
        }


        private bool CheckIfAll_12h_ShiftsAreAssigned(int EmployeeIndex)
        {
            if (ShiftCounter[EmployeeIndex] < requiredFullTimeShifts[EmployeeIndex])
            {
                return true;
            }
            Debug.WriteLine("all 12h shifts returned false");
            
            return false;
        }


        private void CalculateEmployeesWorkingHoures()
        {
            for (int EmployeeIdex = 0; EmployeeIdex < employeeCount; EmployeeIdex++)
            {
                if (employeeList[EmployeeIdex] == null)
                {
                    continue;
                }
                string workingTime = employeeList[EmployeeIdex].WorkingTime;
                double workingHoures;
                if (workingTime.Equals("Pełny etat"))
                {
                    workingHoures = MonthView.CalculateWorkingHoures(eventDays.Count, daysInMonth, MonthView.FullTime);
                }
                else if (workingTime.Equals("1/2 etatu"))
                {
                    workingHoures = MonthView.CalculateWorkingHoures(eventDays.Count, daysInMonth, MonthView.HalfTime);
                }
                else if (workingTime.Equals("3/4 etatu"))
                {
                    workingHoures = MonthView.CalculateWorkingHoures(eventDays.Count, daysInMonth, MonthView.ThreeFourthOfTime);
                }
                else
                {
                    workingHoures = 0.0;
                }

                int houres = (int)workingHoures;
                int minutes = (int)Math.Round((workingHoures - houres) * 60);
                int fullTimeDays = houres / 12;
                int shortShitfHoures = houres - fullTimeDays * 12;

                requiredFullTimeShifts[EmployeeIdex] = fullTimeDays;
                if (shortShitfHoures > 0 || minutes > 0)
                {
                    requiredPartTimeShift[EmployeeIdex] = true;
                }
            }
        }


        private bool CheckIfStill_35h_InFollowing_7_Days(int employeeIndex, int day, string shiftToInsert)
        {
            // copy of previous state
            BitArray copy = new BitArray(monthSchedule[employeeIndex, day].Data);

            // temporary insert this shift
            if (shiftToInsert.Equals("D"))
            {
                monthSchedule[employeeIndex, day].Data[0] = true;
            }
            else if (shiftToInsert.Equals("N"))
            {
                monthSchedule[employeeIndex, day].Data[1] = true;
            }
            else if (shiftToInsert.Equals("d"))
            {
                monthSchedule[employeeIndex, day].Data[0] = true;
                monthSchedule[employeeIndex, day].Data[1] = true;
            }

            int startDay = (day / 7) * 7;
            int endDay = startDay + 7;
            if (endDay > daysInMonth - 1)
            {
                endDay = daysInMonth - 1;
            }
            bool currentDayShiftExist, nextDayShiftExist, doubleNextDayShiftExist, canProceed = false;

            for (int i = startDay; i < endDay - 1; i++)
            {
                // check for D, /
                currentDayShiftExist = monthSchedule[employeeIndex, i].Data[0];
                nextDayShiftExist = monthSchedule[employeeIndex, i + 1].Data[0] | monthSchedule[employeeIndex, i + 1].Data[1];
                if (currentDayShiftExist & !nextDayShiftExist)
                {
                    canProceed = true;
                    Debug.WriteLine("check for:  D, /");
                    break;
                }

                // check for /, /
                currentDayShiftExist = monthSchedule[employeeIndex, i].Data[0] | monthSchedule[employeeIndex, i + 1].Data[1]; 
                nextDayShiftExist = monthSchedule[employeeIndex, i + 1].Data[0] | monthSchedule[employeeIndex, i + 1].Data[1];
                if (!currentDayShiftExist & !nextDayShiftExist)
                {
                    canProceed = true;
                    Debug.WriteLine("check for:  /, /");
                    break;
                }

                if (i < endDay - 2)
                {
                    // check for N, /, N
                    currentDayShiftExist = monthSchedule[employeeIndex, i].Data[1];
                    nextDayShiftExist = monthSchedule[employeeIndex, i + 1].Data[0] | monthSchedule[employeeIndex, i + 1].Data[1];
                    doubleNextDayShiftExist = monthSchedule[employeeIndex, i + 2].Data[1];
                    if (currentDayShiftExist & !nextDayShiftExist & doubleNextDayShiftExist)
                    {
                        canProceed = true;
                        Debug.WriteLine("check for:  N, /, N");
                        break;
                    }
                }
            }

            // rollback previous temporary changes
            monthSchedule[employeeIndex, day].Data = copy;

            if (!canProceed && (endDay - startDay < 4))
            {
                canProceed = true;
                Debug.WriteLine("Allow due to short week (under 4 days)");
            }

            if (!canProceed)
            {
                Debug.WriteLine("35h returned false");
            }
            return canProceed;
        }


        public DepartmentWorkArrangement getSchedule()
        {
            for (int employee_i = 0; employee_i < employeeCount; employee_i++)
            {
                for (int day = 0; day < daysInMonth; day++)
                {
                    if (this.monthSchedule[employee_i, day].Data[0] && this.monthSchedule[employee_i, day].Data[2])
                    {
                        this.modified_dwa.allEmployeeWorkArrangement[employee_i].SetEmployeeWorkArrangement(day + 1, "d");
                    }
                    else if (this.monthSchedule[employee_i, day].Data[0])
                    {
                        this.modified_dwa.allEmployeeWorkArrangement[employee_i].SetEmployeeWorkArrangement(day + 1, "D");
                    }
                    else if (this.monthSchedule[employee_i, day].Data[1])
                    {
                        this.modified_dwa.allEmployeeWorkArrangement[employee_i].SetEmployeeWorkArrangement(day + 1, "N");
                    }
                }
            }
            return this.modified_dwa;
        }


        private void getDataFromPreviousMonth()
        {
            DateTime prevoiusMonth = getPreviousMonthFileName(year, month);
            string prevoiusMonthFileName = "Schedules\\" + prevoiusMonth.Month.ToString() + "_" + prevoiusMonth.Year.ToString() + "_grafik.txt";
            bool fileExist = File.Exists(prevoiusMonthFileName);

            if (!fileExist)
            {
                string messageBoxText = "Nie znaleziono grafiku dla poprzedniego miesiąca. \n" +
                                        "Jeśli chcesz, aby dane na temat rozkładu pracy pracowników z poprzedniego " +
                                        "miesiąca zostały wzięte pod uwagę najpierw dodaj ręcznie poprzedni miesiąc";
                string caption = "Brakujące dane";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
            }
            else
            {
                ScheduleData data = ScheduleFile.ReadExistingSchedule(prevoiusMonthFileName);
                prevoiusMonth_dwa = findMy_dwa(data.ListOfDepartmentsWorkArrangements, department.Id);
                getDataAboutLastDayOfPrevoiusMonth();
                getDataAboutSundaysInPrevoiusMonth();
            }
        }

        private DateTime getPreviousMonthFileName(int year, int month)
        {
            DateTime dt = new DateTime(year, month, 1);
            dt = dt.AddMonths(-1);
            return dt;
        }


        private DepartmentWorkArrangement findMy_dwa(List<DepartmentWorkArrangement> deps, int id)
        {
            List<DepartmentWorkArrangement> filtered = deps.Where(d => d.department.Id.Equals(id)).ToList();
            if (filtered.Count == 1)
            {
                return filtered[0];
            }
                       
            string messageBoxText = "W grafiku z poprzedniego miesiąca nie znaleziono danych dotyczących " +
                                    "aktualnie grafikowanego oddziału. W celu poprawnego działania programu " + 
                                    "uzupełnij brakujące dane.";
            string caption = "Brakujące dane";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);

            return null;
        }


        private void getDataAboutLastDayOfPrevoiusMonth()
        {
            DateTime prevoiusMonth = getPreviousMonthFileName(year, month);
            for (int i = 0; i < employeeCount; i++)
            {
                Employee empl = employeeList[i];
                if (empl == null)
                {
                    continue;
                }
                List<EmployeeWorkArrangement> _ewa = prevoiusMonth_dwa.allEmployeeWorkArrangement.Where(e => e.employee.Id.Equals(empl.Id)).ToList();
                if (_ewa.Count > 0)
                {
                    string lastDayShift = _ewa.First().GetSingleWorkArrangement(DateTime.DaysInMonth(prevoiusMonth.Year, prevoiusMonth.Month));
                    if (lastDayShift == "D")
                    {
                        lastDayOfPreviousMonthSchedule[i].Data[0] = true;
                    }
                    else if (lastDayShift == "d")
                    {
                        lastDayOfPreviousMonthSchedule[i].Data[0] = true;
                        lastDayOfPreviousMonthSchedule[i].Data[2] = true;
                    }
                    else if (lastDayShift == "N")
                    {
                        lastDayOfPreviousMonthSchedule[i].Data[1] = true;
                    }
                }
            }
        }


        private void getDataAboutSundaysInPrevoiusMonth()
        {
            // get data about prevoius month
            DateTime prevoiusMonth = getPreviousMonthFileName(year, month);

            // make list of sundays in previous month
            sundaysListInPrevoiusMonth = new List<int>();
            for (int day = 1; day < DateTime.DaysInMonth(prevoiusMonth.Year, prevoiusMonth.Month); day++)
            {
                DateTime dt = new DateTime(prevoiusMonth.Year, prevoiusMonth.Month, day);
                if (dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    sundaysListInPrevoiusMonth.Add(day);
                }
            }

            // check all emoployees sundays
            for (int i = 0; i < employeeCount; i++)
            {
                Employee empl = employeeList[i];
                if (empl == null)
                {
                    continue;
                }
                List<EmployeeWorkArrangement> _ewa = prevoiusMonth_dwa.allEmployeeWorkArrangement.Where(e => e.employee.Id.Equals(empl.Id)).ToList();
                if (_ewa.Count > 0)
                {
                    sundaysListInPrevoiusMonth.Reverse();
                    foreach (int sunday in sundaysListInPrevoiusMonth)
                    {
                        string lastDayShift = _ewa.First().GetSingleWorkArrangement(sunday);
                        if (lastDayShift == "D" || lastDayShift == "N" || lastDayShift == "d")
                        {
                            numberOfRecentlyWorkedSundays[i] += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    sundaysListInPrevoiusMonth.Reverse();
                }
                else
                {
                    numberOfRecentlyWorkedSundays[i] = 0;
                }
            }
        }


        private void updateQueueWithDays()
        {
            queueOfDaysDay = new List<(int, int)>();
            queueOfDaysNight = new List<(int, int)>();
            for (int day = 0; day < daysInMonth; day++)
            {
                (int _day, int _employeeCount) dayInfo = (day, employeesOnDayShiftCounter[day]);
                (int _day, int _employeeCount) nightInfo = (day, employeesOnNightShiftCounter[day]);

                queueOfDaysDay.Add(dayInfo);
                queueOfDaysNight.Add(nightInfo);
            }
            queueOfDaysDay = queueOfDaysDay.OrderBy(x => x._employeeCount).ToList();
            queueOfDaysNight = queueOfDaysNight.OrderBy(x => x._employeeCount).ToList();
        }
    }
}
