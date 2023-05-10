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
using System.Windows.Documents;

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
        //  |     |--> bit 4 is emplooyee requesting day free
        //  |     |--> bit 5
        //  |     |--> bit 6
        //  |     |--> bit 7 Error
        //  |--> second byte - error code
        
        public BitArray Data;
        public byte Error;


        public shiftData(bool dayShift = false, bool nightShift = false, bool shortShift = false, bool vacationDay = false, bool dayFree = false)
        {
            Data = new BitArray(8, false);
            Data[0] = dayShift;
            Data[1] = nightShift;
            Data[2] = shortShift;
            Data[3] = vacationDay;
            Data[4] = dayFree;
            // Data[5] = ...;
            // Data[6] = ...;
            Data[7] = true; // starting with error corresponging to wrong occupation
            Error = 0x01; 
        }


        public shiftData(shiftData sd)
        {
            Error = sd.Error;
            Data = sd.Data;
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

        public shiftEmployees(shiftEmployees se)
        {
            employeeCount = se.employeeCount;
            occupation = se.occupation;
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
        private DepartmentWorkArrangement requests_dwa;
        private List<EmployeeWorkArrangement> ewa;
        private List<int> eventDays;
        private int daysInMonth;
        private int year;
        private int month;
        private int employeeCount;
        private shiftData[/* employeeCount */, /* daysInMonth */] monthRequests;
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
        private List<int> weekendDays;



        public  Solver (DepartmentWorkArrangement _dwa, List<int> _eventDays, int _year, int _month, DepartmentWorkArrangement requestsDwa)
        {
            dwa = _dwa;
            department = _dwa.department;
            ewa = _dwa.allEmployeeWorkArrangement;
            requests_dwa = requestsDwa;
            eventDays = _eventDays;
            year = _year;
            month = _month;
            daysInMonth = DateTime.DaysInMonth(_year, _month);
            employeeCount = ewa.Count;
            monthSchedule = new shiftData[employeeCount, daysInMonth];
            monthRequests = new shiftData[employeeCount, daysInMonth];
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
                lastDayOfPreviousMonthSchedule[employeeNumber] = new shiftData(false, false, false, false, false);

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
                    bool requestForFreeDay = false;
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
                    monthSchedule[employeeNumber, day] = new shiftData(dayShift, nightShift, shortShift, vacationDay, requestForFreeDay);
                }
            }
            end = employeeCount - 1;
            currentGroup = new employeeGroupIndex(start, end);
            employeeGroupIndices.Add(currentGroup);
            OccupationToGroupIndex.Add(ewa[start].employee.Occupation, employeeGroupIndices.Count - 1);

            int offset = 0;
            // asigninging proper shift data for requests of all employees
            for (int employeeNumber = 0; employeeNumber < employeeCount; employeeNumber++)
            {
                if (employeeList[employeeNumber] == null)
                {
                    offset++;
                    continue;
                }
                List<string> temporaryEmployeeScheduleData = new List<string>(requests_dwa.allEmployeeWorkArrangement[employeeNumber - offset].GetWorkArrangementAsList());
                for (int day = 0; day < daysInMonth; day++)
                {
                    bool dayShift = temporaryEmployeeScheduleData[day].Equals("D") | temporaryEmployeeScheduleData[day].Equals("d");
                    bool nightShift = temporaryEmployeeScheduleData[day].Equals("N");
                    bool shortShift = temporaryEmployeeScheduleData[day].Equals("d");
                    bool requestForFreeDay = temporaryEmployeeScheduleData[day].Equals("/");
                    bool vacationDay = false;
                    if (!(dayShift | nightShift | shortShift))
                    {
                        vacationDay = temporaryEmployeeScheduleData[day].Equals("U")  |
                                      temporaryEmployeeScheduleData[day].Equals("Um") |
                                      temporaryEmployeeScheduleData[day].Equals("Us") |
                                      temporaryEmployeeScheduleData[day].Equals("Uo") |
                                      temporaryEmployeeScheduleData[day].Equals("Uż") |
                                      temporaryEmployeeScheduleData[day].Equals("C")  |
                                      temporaryEmployeeScheduleData[day].Equals("Op");
                    }
                    monthRequests[employeeNumber, day] = new shiftData(dayShift, nightShift, shortShift, vacationDay, requestForFreeDay);
                }
            }

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

            makeWeekendDaysList();
            CalculateEmployeesWorkingHoures();
            getDataFromPreviousMonth();
            GenerateHardCoistrainsCorrectSchedule();
            AmmendScheduleAccordingToSoftConstrains();
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
                        bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "N", day, ref monthSchedule) &
                                          CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day, ref monthSchedule) &
                                          CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup) &
                                          CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "N", ref monthSchedule);

                        if (isThisSunday)
                        {
                            canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "N", ref monthSchedule);
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
                        bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "D", day, ref monthSchedule)      &
                                          CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day, ref monthSchedule)   &
                                          CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup)         &
                                          CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "D", ref monthSchedule);
                        
                        if (isThisSunday)
                        {
                            canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "D", ref monthSchedule);
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

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "D", day, ref monthSchedule) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day, ref monthSchedule) &
                                      CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "D", ref monthSchedule);
                    
                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "D", ref monthSchedule);
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

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "N", day, ref monthSchedule) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day, ref monthSchedule) &
                                      CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumFromCurrentGroup, day, "N", ref monthSchedule);

                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day, "N", ref monthSchedule);
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

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumber, shiftType, day, ref monthSchedule) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumber, day, ref monthSchedule) &
                                      CheckIfAll_12h_ShiftsAreAssigned(employeeNumber) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumber, day, shiftType, ref monthSchedule);

                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumber, day, shiftType, ref monthSchedule);
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

                    bool canProceed = CheckIf_12h_BetweenShifts(employeeNumber, shiftType, day, ref monthSchedule) &
                                      CheckIfExistAllreadyAssignedShift(employeeNumber, day, ref monthSchedule) &
                                      CheckIfStill_35h_InFollowing_7_Days(employeeNumber, day, shiftType, ref monthSchedule);

                    if (isThisSunday)
                    {
                        canProceed &= CheckIfNotFourthSunday(employeeNumber, day, shiftType, ref monthSchedule);
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
            SimullatedAnnealing(100.0, 1.0, 10000, 0.99, "logarithmic");
        }


        private bool CheckIfNotFourthSunday(int employeeIndex , int day, string shiftToInsert, ref shiftData[,] monthSchedule)
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


        private bool CheckIf_12h_BetweenShifts(int EmployeeIndex, string ShiftType, int day, ref shiftData[,] monthSchedule)
        {
            if (day > 0)
            {
                // check on current month data
                if (ShiftType.Equals("D"))
                {
                    bool nightBefore = monthSchedule[EmployeeIndex, day - 1].Data[1];
                    return !nightBefore;
                }
                if (ShiftType.Equals("N"))
                {
                    bool currentDay = monthSchedule[EmployeeIndex, day].Data[0];
                    bool daytAfter = false;
                    if (day < daysInMonth - 1)
                    {
                        daytAfter = monthSchedule[EmployeeIndex, day + 1].Data[0];
                    }  
                    return !currentDay & !daytAfter;
                }
                if (ShiftType.Equals("d"))
                {
                    bool nightBefore = monthSchedule[EmployeeIndex, day - 1].Data[1];
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
                    bool dayAfter = monthSchedule[EmployeeIndex, 1].Data[0];
                    return !currentDay & !dayAfter;
                }
                if (ShiftType.Equals("d"))
                {
                    bool nightBefore = this.lastDayOfPreviousMonthSchedule[EmployeeIndex].Data[1];
                    return !nightBefore;
                }
            }
            return false;
        }


        private bool CheckIfExistAllreadyAssignedShift(int EmployeeIndex, int day, ref shiftData[,] monthSchedule)
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


        private bool CheckIfStill_35h_InFollowing_7_Days(int employeeIndex, int day, string shiftToInsert, ref shiftData[,] monthSchedule)
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
                    // Debug.WriteLine("check for:  D, /");
                    break;
                }

                // check for /, /
                currentDayShiftExist = monthSchedule[employeeIndex, i].Data[0] | monthSchedule[employeeIndex, i + 1].Data[1]; 
                nextDayShiftExist = monthSchedule[employeeIndex, i + 1].Data[0] | monthSchedule[employeeIndex, i + 1].Data[1];
                if (!currentDayShiftExist & !nextDayShiftExist)
                {
                    canProceed = true;
                    // Debug.WriteLine("check for:  /, /");
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
                        // Debug.WriteLine("check for:  N, /, N");
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

            // place weekend days higher in List -> less people during weekends
            for (int i = 0; i < daysInMonth - weekendDays.Count; i++)
            {
                bool decrement = false;
                if (weekendDays.Contains(queueOfDaysDay[i]._day + 1))
                {
                    (int _day, int _employeeCount) tempInfo = queueOfDaysDay[i];
                    queueOfDaysDay.RemoveAt(i);
                    queueOfDaysDay.Add(tempInfo);
                    decrement = true;
                }

                if (weekendDays.Contains(queueOfDaysNight[i]._day + 1))
                {
                    (int _day, int _employeeCount) tempInfo = queueOfDaysNight[i];
                    queueOfDaysNight.RemoveAt(i);
                    queueOfDaysNight.Add(tempInfo);
                    decrement = true;
                }

                if (decrement)
                {
                    i--;
                }
            }
        }


        private void makeWeekendDaysList()
        {
            weekendDays = new List<int>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime dt = new DateTime(year, month, day);
                if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday || eventDays.Contains(day))
                {
                    weekendDays.Add(day);
                }
            }
        }


        private void SimullatedAnnealing(double initialTemeprature, double minTemperature, int maxIterations, double k, string tempType)
        {
            // copy of ooryginal first solution
            shiftData[,] oldMonthSchedule = monthSchedule.Clone() as shiftData[,];
            shiftData[,] bestSolution = monthSchedule.Clone() as shiftData[,];
            shiftData[,] bestEverSolution = monthSchedule.Clone() as shiftData[,];
            shiftData[,] solution;

            double temperature = initialTemeprature;
            double bestQuality = Quality(oldMonthSchedule);
            double bestEverQuality = bestQuality;
            double startQuality = bestQuality;
            double endQuality;
            double delta;
            double currentQuality;
            int iteration = 0;
            bool acceptSolution = false;
            bool canContiniue = true;    
            Random rand = new Random();
            
            while (canContiniue)
            {
                // Debug.WriteLine("iteration: " + iteration.ToString() + " temp: " + temperature.ToString());
                
                // generate solution
                solution = generateSolutionFromNeighbourhood(bestSolution);
                
                // calculate it's Quality
                currentQuality = Quality(solution);

                delta = currentQuality - bestQuality;

                if (delta > 0.0)
                {
                    acceptSolution = true;
                }
                else
                {
                    double prop = Math.Exp(delta / (k * temperature));
                    Debug.WriteLine(prop.ToString());
                    if (rand.NextDouble() > prop)
                    {
                        acceptSolution = true;
                    }
                }

                if (acceptSolution)
                {
                    bestSolution = solution.Clone() as shiftData[,];
                    bestQuality = currentQuality;
                    if (bestQuality > bestEverQuality)
                    {
                        bestEverQuality = bestQuality;
                        bestEverSolution = bestSolution.Clone() as shiftData[,];
                    }
                    acceptSolution = false;
                    Debug.WriteLine("New solution at: " + iteration.ToString() + " iteration, with quality of: " + bestQuality.ToString());
                }

                // change tempperature, increase iterations and check finishing factor
                temperature = newTemperature(temperature, iteration, 0.99, tempType);
                if (temperature < minTemperature)
                {
                    temperature = minTemperature;
                }

                iteration++;
                if (iteration >= maxIterations)
                {
                    canContiniue = false;
                }
            }
            monthSchedule = bestEverSolution.Clone() as shiftData[,];
            endQuality = bestQuality;
            Debug.WriteLine("=================================== DONE ===================================");

            string messageBoxText = "Start Quality: " + startQuality.ToString("0.0") + "\n" +
                                    "End Quality: " + endQuality.ToString("0.0") + "\n" +
                                    "Best Ever Quality: " + bestEverQuality.ToString("0.0");

            string caption = "Quality check";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
        }


        private shiftData[,] generateSolutionFromNeighbourhood(shiftData[,] sd)
        {
            // copy item to target
            shiftData[,] solution;

            // do something random to generate ALLOWED solution
            Random rand = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
            
            // random chooose employee
            int randomEmployee;
            
            bool canProceed = false;
            int day1, day2, oldShiftCounterValue;
            BitArray tempData;
            string day1ShiftType = ""; 
            string day2ShiftType = "";

            do
            {
                // copy item to target
                solution = sd.Clone() as shiftData[,];
                
                // radnom choose employee
                do  
                {
                    randomEmployee = rand.Next(0, employeeCount);
                }
                while (employeeList[randomEmployee] == null);

                // random choose days to change shifts
                day1 = rand.Next(0, daysInMonth);      
                do
                {
                    day2 = rand.Next(0, daysInMonth);
                }
                while (solution[randomEmployee, day1].Data == solution[randomEmployee, day2].Data);

                // check if there are any shifts in those days
                bool day1ShiftsExist = sd[randomEmployee, day1].Data[0] | sd[randomEmployee, day1].Data[1];
                bool day2ShiftsExist = sd[randomEmployee, day2].Data[0] | sd[randomEmployee, day2].Data[1];

                // if there is no shifts then continiue
                if (!(day1ShiftsExist | day2ShiftsExist))
                {
                    continue;
                }

                // force changes, and check if they are coorrect
                tempData = solution[randomEmployee, day1].Data;
                solution[randomEmployee, day1].Data = solution[randomEmployee, day2].Data;
                solution[randomEmployee, day2].Data = tempData;

                canProceed = checkIfChangesAreLegal(day1, day2, randomEmployee, ref solution);
            }
            while (!canProceed);

            return solution;
        }


        private double newTemperature(double currentTemperature, int currentIteration, double k, string version)
        {
            double temperature = currentTemperature;

            switch (version)
            {
                case "linear":
                    temperature -= k;
                    break;

                case "logarithmic":
                    temperature *= Math.Pow(k, currentIteration);
                    break;

                case "geometric":
                    temperature *= k;
                    break;
            }
            return temperature;
        }


        private double Quality(shiftData[,] A)
        {
            int initialPoints = 5000;
            int pointsToAdd = 0;
            int dayComboCounter;
            int nightComboCounter;
            int bothComboCounter;
            int[] employeeCount_Working_Day = Enumerable.Repeat<int>(0, daysInMonth).ToArray();
            int[] employeeCount_Working_Night = Enumerable.Repeat<int>(0, daysInMonth).ToArray();
            int[] employeeCount_nonWorking_Day = Enumerable.Repeat<int>(0, daysInMonth).ToArray();
            int[] employeeCount_nonWorking_Night = Enumerable.Repeat<int>(0, daysInMonth).ToArray();

            // check fitting to employees requests
            // check for combinations bigger than D, D, D, ... or N, N, ...

            for (int empl_i = 0; empl_i < employeeCount; empl_i++)
            {
                if (employeeList[empl_i] == null)
                {
                    continue;
                }
                dayComboCounter = 0;
                nightComboCounter = 0;
                bothComboCounter = 0;
                for (int day_i = 0; day_i < daysInMonth; day_i++)
                {
                    // check for combo in D
                    if (A[empl_i, day_i].Data[0] == true)
                    {
                        dayComboCounter++;
                        bothComboCounter++;
                    }
                    else
                    {
                        if (dayComboCounter > 2)
                        {
                            pointsToAdd -= 30 * (dayComboCounter - 2);
                        }
                        dayComboCounter = 0;
                    }

                    // // check for combo in N
                    if (A[empl_i, day_i].Data[1] == true)
                    {
                        nightComboCounter++;
                    }
                    else
                    {
                        if (nightComboCounter > 1)
                        {
                            pointsToAdd -= 30 * (nightComboCounter - 1);
                        }
                        nightComboCounter = 0;
                    }

                    if (A[empl_i, day_i].Data[0] == true || A[empl_i, day_i].Data[1] == true)
                    {
                        bothComboCounter++;
                    }
                    else
                    {
                        if (bothComboCounter > 3)
                        {
                            pointsToAdd -= 100 * (bothComboCounter - 3);
                        }
                        bothComboCounter = 0;
                    }

                    // calculate people per shift
                    if (weekendDays.Contains(day_i + 1))
                    {
                        if (A[empl_i, day_i].Data[0] == true) employeeCount_nonWorking_Day[day_i]++;
                        if (A[empl_i, day_i].Data[1] == true) employeeCount_nonWorking_Night[day_i]++;
                    }
                    else
                    {
                        if (A[empl_i, day_i].Data[0] == true) employeeCount_Working_Day[day_i]++;
                        if (A[empl_i, day_i].Data[1] == true) employeeCount_Working_Night[day_i]++;
                    }

                    // \ is equal -> +10pkt
                    if (monthRequests[empl_i, day_i].Data[4] && A[empl_i, day_i].Data[0] == false && A[empl_i, day_i].Data[1] == false)
                    {
                        pointsToAdd += 100;
                        // continue;
                    }
                    // D is equal -> +8pkt
                    if (A[empl_i, day_i].Data[0] && monthRequests[empl_i, day_i].Data[0])
                    {
                        pointsToAdd += 80;
                        // continue;
                    }
                    // N is equal -> +7pkt
                    if (A[empl_i, day_i].Data[1] && monthRequests[empl_i, day_i].Data[1])
                    {
                        pointsToAdd += 70;
                    }
                }
            }

            List<int> _employeeCount_Working_Day = new List<int>();
            List<int> _employeeCount_Working_Night = new List<int>();
            List<int> _employeeCount_nonWorking_Day = new List<int>();
            List<int> _employeeCount_nonWorking_Night = new List<int>();
            foreach (int day in employeeCount_Working_Day) if (day != 0) _employeeCount_Working_Day.Add(day);
            foreach (int day in employeeCount_Working_Night) if (day != 0) _employeeCount_Working_Night.Add(day);
            foreach (int day in employeeCount_nonWorking_Day) if (day != 0) _employeeCount_nonWorking_Day.Add(day);
            foreach (int day in employeeCount_nonWorking_Night) if (day != 0) _employeeCount_nonWorking_Night.Add(day);

            pointsToAdd -= 5 * (int)StandardDeviation(_employeeCount_Working_Day);
            pointsToAdd -= 5 * (int)StandardDeviation(_employeeCount_Working_Night);
            pointsToAdd -= 5 * (int)StandardDeviation(_employeeCount_nonWorking_Day);
            pointsToAdd -= 5 * (int)StandardDeviation(_employeeCount_nonWorking_Night);
           
            return initialPoints + pointsToAdd;
        }


        private double StandardDeviation(List<int> arr)
        {
            int sum = 0;
            foreach (int item in arr)
            {
                sum += item;
            }
            double avg = sum / arr.Count;

            double sumOfSquares = 0;
            foreach (int item in arr)
            {
                sumOfSquares += Math.Pow(item - avg, 2);
            }
            return Math.Sqrt(sumOfSquares / arr.Count);
        }
    
        
        private bool CheckIfStillEnoughOfFundamentalEmployees(int employeeIndex, int day, ref shiftData[,] monthSchedule)
        {
            dayEmployees de = new dayEmployees(expected_fundamentalEmplyees);

            for (int i = 0; i < de.employees_Day.Count; i++)
            {
                for (int employee_i = 0; employee_i < employeeCount; employee_i++)
                {
                    if (employeeList[employee_i] == null)
                    {
                        continue;
                    }

                    if (monthSchedule[employee_i, day].Data[0] == true && monthSchedule[employee_i, day].Data[2] == false)
                    {
                        if (employeeList[employee_i].Occupation == de.employees_Day[i].occupation)
                        {
                            de.employees_Day[i].employeeCount--;
                        }
                    }
                }
            }

            for (int i = 0; i < de.employees_Night.Count; i++)
            {
                for (int employee_i = 0; employee_i < employeeCount; employee_i++)
                {
                    if (employeeList[employee_i] == null)
                    {
                        continue;
                    }

                    if (monthSchedule[employee_i, day].Data[1] == true)
                    {
                        if (employeeList[employee_i].Occupation == de.employees_Night[i].occupation)
                        {
                            de.employees_Night[i].employeeCount--;
                        }
                    }
                }
            }

            foreach (shiftEmployees se in de.employees_Day)
            {
                // Debug.WriteLine("=========================================== Day: " + se.occupation + ": " + se.employeeCount);
                if (se.employeeCount > 1)
                {
                    return false;
                }
            }

            foreach (shiftEmployees se in de.employees_Night)
            {
                // Debug.WriteLine("=========================================== Night: " + se.occupation + ": " + se.employeeCount);
                if (se.employeeCount > 1)
                {
                    return false;
                }
            }
            return true;
        }


        private bool checkIfChangesAreLegal(int day1, int day2, int employeeIndex, ref shiftData[,] monthSchedule)
        {          
            string day1ShiftType = getShiftType(monthSchedule[employeeIndex, day1].Data);
            string day2ShiftType = getShiftType(monthSchedule[employeeIndex, day2].Data);

            // check day1
            if (day1ShiftType != "")
            {
                // check if 12h breaks still exist
                if (!CheckIf_12h_BetweenShifts(employeeIndex, day1ShiftType, day1, ref monthSchedule))
                {
                    return false;
                }

                // check if 35 houres still exist
                if (!CheckIfStill_35h_InFollowing_7_Days(employeeIndex, day1, day1ShiftType, ref monthSchedule))
                {
                    return false;
                }
            }
            // check if still enough Fundamental employees
            if (!CheckIfStillEnoughOfFundamentalEmployees(employeeIndex, day1, ref monthSchedule))
            {
                return false;
            }

            // check day2
            if (day2ShiftType != "")
            {
                // check if 12h breaks still exist
                if (!CheckIf_12h_BetweenShifts(employeeIndex, day2ShiftType, day2, ref monthSchedule))
                {
                    return false;
                }

                // check if 35 houres still exist
                if (!CheckIfStill_35h_InFollowing_7_Days(employeeIndex, day2, day2ShiftType, ref monthSchedule))
                {
                    return false;
                }
            }
            // check if still enough Fundamental employees
            if (!CheckIfStillEnoughOfFundamentalEmployees(employeeIndex, day2, ref monthSchedule))
            {
                return false;
            }

            return true;
        }


        private string getShiftType(BitArray Data)
        {
            string day1ShiftType;
            if (Data[0] && Data[2])
            {
                day1ShiftType = "d";
            }
            else if (Data[0])
            {
                day1ShiftType = "D";
            }
            else if (Data[1])
            {
                day1ShiftType = "N";
            }
            else
            {
                day1ShiftType = "";
            }
            return day1ShiftType;
        }


        private void TabuSearch(int maxIterations, int tabuListLength)
        {
            // copy of ooryginal first solution
            shiftData[,] oldMonthSchedule = monthSchedule.Clone() as shiftData[,];
            shiftData[,] bestEverSolution = monthSchedule.Clone() as shiftData[,];
            shiftData[,] solution;

            Neighbour localBestNeighbour = new Neighbour(bestEverSolution, (0, 0, 0, "", ""));
            Neighbour BestEverNeighbour = new Neighbour(localBestNeighbour);
            

            double localBestQuality = Quality(oldMonthSchedule);
            double localBestNeighbourQuality = localBestQuality;
            double bestEverQuality = localBestQuality;
            double startQuality = localBestQuality;
            double endQuality;
            double currentQuality;

            // list of neighbours
            List<Neighbour> neighbourhood;
            
            // Tabu List
            TabuList tabuList = new TabuList(tabuListLength);
            
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // get neighbourhood for current local best solution
                neighbourhood = getNeighbourhoodOf(localBestNeighbour.schedule);

                localBestNeighbour = neighbourhood[0];
                localBestNeighbourQuality = Quality(localBestNeighbour.schedule);
                for (int i = 1; i < neighbourhood.Count; i++)
                {
                    // measure quality
                    currentQuality = Quality(neighbourhood[i].schedule);

                    // check if changes are on tabu list
                    if (!tabuList.Contains(neighbourhood[i].changes))
                    {
                        if (currentQuality > localBestNeighbourQuality)
                        {
                            localBestNeighbour = neighbourhood[i];
                            localBestNeighbourQuality = currentQuality;
                            tabuList.Add(localBestNeighbour.changes);
                            Debug.WriteLine("Found better solution with quality: " + localBestNeighbourQuality.ToString());
                        }
                    }
                    
                    if (currentQuality > bestEverQuality)
                    {
                        BestEverNeighbour = neighbourhood[i];
                        bestEverQuality = currentQuality;
                    }   
                }
            }
            endQuality = localBestNeighbourQuality;
            monthSchedule = BestEverNeighbour.schedule.Clone() as shiftData[,];
            Debug.WriteLine("=================================== TABU SEARCH DONE ===================================");

            string messageBoxText = "Start Quality: " + startQuality.ToString("0.0") + "\n" +
                                    "End Quality: " + endQuality.ToString("0.0") + "\n" +
                                    "Best Ever Quality: " + bestEverQuality.ToString("0.0");

            string caption = "Quality check";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
        }


        class TabuList
        {
            private List<(int emmployeeIndex, int day1, int day2, string day1Shift, string day2Shift)> list;
            private int maxLength;

            public TabuList(int maxLength)
            {
                this.maxLength = maxLength;
                list = new List<(int emmployeeIndex, int day1, int day2, string day1Shift, string day2Shift)>();
            }

            public void Add((int emmployeeIndex, int day1, int day2, string day1Shift, string day2Shift) item)
            {
                if (list.Count == maxLength)
                {
                    list.RemoveAt(maxLength - 1);
                    list.Add(item);
                }
                else
                {
                    list.Add(item);
                }
            }

            public bool Contains((int emmployeeIndex, int day1, int day2, string day1Shift, string day2Shift) item)
            {
                return list.Contains(item);
            }
        }


        class Neighbour
        {
            public shiftData[,] schedule;
            public (int emmployeeIndex, int day1, int day2, string day1Shift, string day2Shift) changes;

            public Neighbour(shiftData[,] schedule, (int emmployeeIndex, int day1, int day2, string day1Shift, string day2Shift) changes)
            {
                this.schedule = schedule.Clone() as shiftData[,];
                this.changes = changes;
            }

            public Neighbour(Neighbour n)
            {
                this.schedule = n.schedule.Clone() as shiftData[,];
                this.changes = n.changes;
            }
        }

        private List<Neighbour> getNeighbourhoodOf(shiftData[,] sd)
        {
            List<Neighbour> neighbours = new List<Neighbour>();
            Neighbour neighbourToAdd;

            bool canProceed = false;
            int day1, day2, oldShiftCounterValue;
            BitArray tempData;
            string day1ShiftType = "";
            string day2ShiftType = "";

            // single solution
            shiftData[,] solution;

            // do something random to generate ALLOWED solution
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            // random chooose employee
            int randomEmployee;

            for (int employee_i = 0; employee_i < employeeList.Count; employee_i++)
            {
                if (employeeList[employee_i] == null)
                {
                    continue;
                }




            }

            do
            {
                // copy item to target
                solution = sd.Clone() as shiftData[,];

                // radnom choose employee
                do
                {
                    randomEmployee = rand.Next(0, employeeCount);
                }
                while (employeeList[randomEmployee] == null);

                // random choose days to change shifts
                day1 = rand.Next(0, daysInMonth);
                do
                {
                    day2 = rand.Next(0, daysInMonth);
                }
                while (solution[randomEmployee, day1].Data == solution[randomEmployee, day2].Data);

                // check if there are any shifts in those days
                bool day1ShiftsExist = sd[randomEmployee, day1].Data[0] | sd[randomEmployee, day1].Data[1];
                bool day2ShiftsExist = sd[randomEmployee, day2].Data[0] | sd[randomEmployee, day2].Data[1];

                // if there is no shifts then continiue
                if (!(day1ShiftsExist | day2ShiftsExist))
                {
                    continue;
                }

                // force changes, and check if they are coorrect
                tempData = solution[randomEmployee, day1].Data;
                solution[randomEmployee, day1].Data = solution[randomEmployee, day2].Data;
                solution[randomEmployee, day2].Data = tempData;

                canProceed = checkIfChangesAreLegal(day1, day2, randomEmployee, ref solution);
            }
            while (!canProceed);

            return neighbours;
        }
    }
}