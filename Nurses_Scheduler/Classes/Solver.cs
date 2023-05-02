﻿using Nurses_Scheduler.Classes.DataBaseClasses;
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
        private int requiredFullTimeShifts;
        private bool requiredPartTimeShift;
        private List<FundamentalEmployee> fundamentalEmployees_Day;
        private List<FundamentalEmployee> fundamentalEmployees_Night;
        private List<ComplementaryEmployee> complementaryEmployess_Day;
        private List<ComplementaryEmployee> complementaryEmployess_Night;
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



        public  Solver (DepartmentWorkArrangement _dwa, List<int> _eventDays, int _year, int _month, int _requiredFullTimeShifts, bool _requiredPartTimeShift) 
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
            requiredFullTimeShifts = _requiredFullTimeShifts;
            requiredPartTimeShift = _requiredPartTimeShift;
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

            this.modified_dwa = new DepartmentWorkArrangement(this.dwa);

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
                                          CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup);
                        
                        if (isThisMonday)
                        {
                            updateSundayList(day);
                        }
                        if (isThisSunday)
                        {
                            canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day);
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

                for (int gruop_id = 0; gruop_id < my_fundamentalEmplyees[day].employees_Night.Count; gruop_id++)
                {
                    // if on given day are missing complementary employees of type 0 (pielęgniarki)
                    while (my_fundamentalEmplyees[day].employees_Night[gruop_id].employeeCount < expected_fundamentalEmplyees.employees_Night[gruop_id].employeeCount)
                    {
                        // random chose number of employee froom group of 0 (pielęgniarki)
                        string occupation = expected_fundamentalEmplyees.employees_Night[gruop_id].occupation;
                        int employeeNumFromCurrentGroup = rand.Next(employeeGroupIndices[OccupationToGroupIndex[occupation]].start, employeeGroupIndices[OccupationToGroupIndex[occupation]].end + 1);
                        bool canProceed = CheckIf_12h_BetweenShifts(employeeNumFromCurrentGroup, "N", day)      &
                                          CheckIfExistAllreadyAssignedShift(employeeNumFromCurrentGroup, day)   &
                                          CheckIfAll_12h_ShiftsAreAssigned(employeeNumFromCurrentGroup);
                        if (isThisMonday)
                        {
                            updateSundayList(day);
                        }
                        if (isThisSunday)
                        {
                            canProceed &= CheckIfNotFourthSunday(employeeNumFromCurrentGroup, day);
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

                Debug.WriteLine("Pracownicy podstawowi uzupełnieni");
                




            }
        }


        private void AmmendScheduleAccordingToSoftConstrains()
        {

        }


        private bool CheckIfNotFourthSunday(int EmployeeIndex , int day)
        {
            return !(numberOfRecentlyWorkedSundays[EmployeeIndex] > 3);
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
                    return !currentDay;
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
                    return !currentDay;
                }
                if (ShiftType.Equals("d"))
                {
                    bool nightBefore = this.lastDayOfPreviousMonthSchedule[EmployeeIndex].Data[1];
                    return !nightBefore;
                }
            }
            return false;
        }


        private bool CheckIfExistAllreadyAssignedShift(int EmployeeIndex, int day)
        {
            bool currentDay = this.monthSchedule[EmployeeIndex, day].Data[0];
            bool currentNight = this.monthSchedule[EmployeeIndex, day].Data[1];   
            return !(currentDay | currentNight);
        }


        private bool CheckIfAll_12h_ShiftsAreAssigned(int EmployeeIndex)
        {
            if (ShiftCounter[EmployeeIndex] < this.requiredFullTimeShifts)
            {
                return true;
            }
            return false;
        }


        public DepartmentWorkArrangement getSchedule()
        {
            for (int employee_i = 0; employee_i < employeeCount; employee_i++)
            {
                for (int day = 0; day < daysInMonth; day++)
                {
                    if (this.monthSchedule[employee_i, day].Data[0])
                    {
                        this.modified_dwa.allEmployeeWorkArrangement[employee_i].SetEmployeeWorkArrangement(day + 1, "D");
                    }
                    if (this.monthSchedule[employee_i, day].Data[1])
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


        private void updateSundayList(int dayUpToWhichUpdate)
        {
            sundaysList.Reverse();
            
            for (int i = 0; i < employeeCount; i++)
            {
                numberOfRecentlyWorkedSundays[i] = 0;
            }

            bool checkInPreviousMonth = true;

            foreach (int sunday_i in sundaysList)
            {
                if (sunday_i <= dayUpToWhichUpdate)
                {
                    checkInPreviousMonth = false;
                    for (int employee_i = 0; employee_i < employeeCount; employee_i++)
                    {
                        if (this.monthSchedule[employee_i, sunday_i].Data[0] || this.monthSchedule[employee_i, sunday_i].Data[1])
                        {
                            if (sunday_i == sundaysList.First())
                            {
                                numberOfRecentlyWorkedSundays[employee_i] = 1;
                            }
                            else
                            {
                                if (numberOfRecentlyWorkedSundays[employee_i] > 0)
                                {
                                    numberOfRecentlyWorkedSundays[employee_i] += 1;
                                }
                                else
                                {
                                    numberOfRecentlyWorkedSundays[employee_i] = 0;
                                }
                            }
                        }
                    }
                }
            }

            if (checkInPreviousMonth)
            {
                getDataAboutSundaysInPrevoiusMonth();
            }

            sundaysList.Reverse();
        }
    }
}
