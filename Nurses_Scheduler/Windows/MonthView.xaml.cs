﻿using Nurses_Scheduler.Classes;
using Nurses_Scheduler.Classes.DataBaseClasses;
using Nurses_Scheduler.Classes.Raport;
using Nurses_Scheduler.Classes.RaportClases;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nurses_Scheduler.Windows
{
    /// <summary>
    /// Interaction logic for MonthView.xaml
    /// </summary>
    public partial class MonthView : Window
    {
        private bool isUserEnteringRequests;
        private int currentMonth;
        private int currentYear;
        private int howManyMonthToShow;
        private List<String> monthsToChoose;

        private int choosenMonth;
        private int choosenYear;
        private int daysInMonth;
        private List<int> eventDays;
        private List<int> forbiddenClickRows;
        private double workingHoures;

        private IDictionary<string, int> OccupationToIndex = new Dictionary<string, int>();
        private IDictionary<string, int> DepartmentToIndex = new Dictionary<string, int>();
        private List<Department> departmentList;
        private List<DepartmentWorkArrangement> departmentsWorkArrangement;

        public static int FullTime = 0;
        public static int HalfTime = 1;
        public static int ThreeFourthOfTime = 2;


        public MonthView(bool isUserEnteringRequests)
        {
            this.isUserEnteringRequests = isUserEnteringRequests;
            InitializeComponent();

            if (isUserEnteringRequests)
            {
                SaveSchedule_Button.Content = "ZAPISZ PROŚBY";
                GenerateSchedule_Button.IsEnabled = false;
                GenerateSchedule_Button.Visibility = Visibility.Hidden;
                GenerateRaport_Button.IsEnabled = false;
                GenerateRaport_Button.Visibility = Visibility.Hidden;
                separator_3.Visibility = Visibility.Hidden;
                separator_4.Visibility = Visibility.Hidden;
            }
            else
            {
                SaveSchedule_Button.Content = "ZAPISZ GRAFIK";
                GenerateSchedule_Button.IsEnabled = true;
                GenerateSchedule_Button.Visibility = Visibility.Visible;
                GenerateRaport_Button.IsEnabled = true;
                GenerateRaport_Button.Visibility = Visibility.Visible;
                separator_3.Visibility = Visibility.Visible;
                separator_4.Visibility = Visibility.Visible;
            }

            currentMonth = DateTime.Now.Month;
            currentYear = DateTime.Now.Year;
            choosenMonth = currentMonth;
            choosenYear = currentYear;
            eventDays = new List<int>();
            howManyMonthToShow = 3;
            monthsToChoose = new List<string>();
            forbiddenClickRows = new List<int>();

            departmentList = Department.GetDepartmentsFromDB();
            for (int i = 0; i < departmentList.Count; i++)
            {
                DepartmentToIndex.Add(departmentList[i].ToString(), i);
            }

            for (int i = 0; i < App.AllowedOccupations.Length; i++)
            {
                OccupationToIndex.Add(App.AllowedOccupations[i], i);
            }

            departmentsWorkArrangement = new List<DepartmentWorkArrangement>();

            InitialiseMonthComboBox();
            InitialiseDepartmentComboBox();
            ShowDepartment_Button.IsEnabled = false;
            GenerateSchedule_Button.IsEnabled = false;
            GenerateRaport_Button.IsEnabled = false;
            Department_ComboBox.IsEnabled = false;
            SaveSchedule_Button.IsEnabled = false;
            workingHoures_TextBox.IsReadOnly = true;
            workingHoures_TextBox.TextWrapping = TextWrapping.Wrap;
        }

        private void InitialiseDepartmentComboBox()
        {
            List<Department> departmentToChoose = Department.GetDepartmentsFromDB();
            Department_ComboBox.ItemsSource = departmentToChoose;
            Department_ComboBox.SelectedIndex = 0;
        }
        private void InitialiseDepartmentComboBox(List<Department> departments)
        {
            Department_ComboBox.ItemsSource = departments;
            Department_ComboBox.SelectedIndex = 0;
        }

        private void InitialiseMonthComboBox()
        {
            for (int i = 0; i < howManyMonthToShow; i++)
            {
                int monthToAdd = currentMonth + i;
                int yearToShow = currentYear;

                // if the next month exceeds december, then substract 12, and ad 1 to current year
                if (monthToAdd > 12)
                {
                    monthToAdd -= 12;
                    yearToShow = currentYear + 1;
                }
                monthsToChoose.Add(App.months[monthToAdd - 1] + " " + yearToShow.ToString());
            }
            ChoosenMonth_ComboBox.ItemsSource = monthsToChoose;
            ChoosenMonth_ComboBox.SelectedIndex = 0;
        }

        private void GenerateNewMonthView()
        {
            departmentsWorkArrangement = new List<DepartmentWorkArrangement>();

            // create datagrid headers according to days in month
            List<string> headers = new List<string>();
            headers.Add("Pracownik");
            for (int i = 0; i < daysInMonth; i++)
            {
                headers.Add((i + 1).ToString());
            }

            // clear dataGrid before inserting new month view
            MonthGrid_DataGrid.ItemsSource = null;
            MonthGrid_DataGrid.Columns.Clear();

            // make proper collumns basing on prevoiusly made headers list
            for (int i = 0; i < headers.Count; i++)
            {
                DataGridTextColumn t1 = new DataGridTextColumn();
                t1.Header = headers[i];
                t1.Binding = new Binding("_" + headers[i]);

                MonthGrid_DataGrid.Columns.Add(t1);

                if (i == 0)
                {
                    MonthGrid_DataGrid.Columns[i].Width = new DataGridLength(20, DataGridLengthUnitType.Star);
                }
                else
                {
                    MonthGrid_DataGrid.Columns[i].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
            }
            
            forbiddenClickRows = new List<int>();


            foreach (Department department in departmentList)
            {
                DepartmentWorkArrangement departmentWorkArrangement = new DepartmentWorkArrangement(department);
                foreach (String occupation in App.AllowedOccupations)
                {
                    List<Employee> employeeList = Employee.GetEmployeesFromDB(occupation, department.Id);
                    if (employeeList.Count > 0)
                    {
                        forbiddenClickRows.Add(departmentWorkArrangement.allEmployeeWorkArrangement.Count);
                        departmentWorkArrangement.allEmployeeWorkArrangement.Add(new EmployeeWorkArrangement(occupation));
                        List<EmployeeWorkArrangement> employeeWorkArrangement = new List<EmployeeWorkArrangement>();
                        foreach (Employee employee in employeeList)
                        {
                            departmentWorkArrangement.allEmployeeWorkArrangement.Add(new EmployeeWorkArrangement(employee));
                        }
                    }
                }  
                departmentsWorkArrangement.Add(departmentWorkArrangement);
            }

            MonthGrid_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].allEmployeeWorkArrangement;

            FindEventDaysInMonth(daysInMonth);
            SetPropertiesForDataGrids();
        }

        private void GenerateNewMonthView(ScheduleData scheduleData)
        {
            departmentsWorkArrangement = new List<DepartmentWorkArrangement>();

            // create datagrid headers according to days in month
            List<string> headers = new List<string>();
            headers.Add("Pracownik");
            for (int i = 0; i < daysInMonth; i++)
            {
                headers.Add((i + 1).ToString());
            }

            // clear dataGrid before inserting new month view
            MonthGrid_DataGrid.ItemsSource = null;
            MonthGrid_DataGrid.Columns.Clear();

            // make proper collumns basing on prevoiusly made headers list
            for (int i = 0; i < headers.Count; i++)
            {
                DataGridTextColumn t1 = new DataGridTextColumn();
                t1.Header = headers[i];
                t1.Binding = new Binding("_" + headers[i]);

                MonthGrid_DataGrid.Columns.Add(t1);
                
                // to make it look okay on different screen sizes
                if (i == 0)
                {
                    MonthGrid_DataGrid.Columns[i].Width = new DataGridLength(20, DataGridLengthUnitType.Star);
                }
                else
                {
                    MonthGrid_DataGrid.Columns[i].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
            }
            
            forbiddenClickRows = new List<int>();

            List<Department> departmentList = new List<Department>();
            DepartmentToIndex.Clear();


            // here I need to finish translating ScheduleData into monthView - propably done - check it
            for (int j = 0; j < scheduleData.ListOfDepartmentsWorkArrangements.Count; j++)
            {
                DepartmentToIndex.Add(scheduleData.ListOfDepartmentsWorkArrangements[j].department.ToString(), j);
                departmentList.Add(scheduleData.ListOfDepartmentsWorkArrangements[j].department);

                List<EmployeeWorkArrangement> AllEmployeeWorkArrangement = scheduleData.ListOfDepartmentsWorkArrangements[j].allEmployeeWorkArrangement;
                string previousOccupation = AllEmployeeWorkArrangement[0].employee.Occupation;

                scheduleData.ListOfDepartmentsWorkArrangements[j].allEmployeeWorkArrangement.Insert(0, new EmployeeWorkArrangement(previousOccupation));
                forbiddenClickRows.Add(0);

                for (int i = 1; i < scheduleData.ListOfDepartmentsWorkArrangements[j].allEmployeeWorkArrangement.Count; i++)
                {
                    EmployeeWorkArrangement ewa = scheduleData.ListOfDepartmentsWorkArrangements[j].allEmployeeWorkArrangement[i];
                    if (ewa.employee.Occupation != previousOccupation)
                    {
                        scheduleData.ListOfDepartmentsWorkArrangements[j].allEmployeeWorkArrangement.Insert(i, new EmployeeWorkArrangement(ewa.employee.Occupation));
                        forbiddenClickRows.Add(i);
                    }
                    previousOccupation = ewa.employee.Occupation;
                }
            }
            departmentsWorkArrangement = scheduleData.ListOfDepartmentsWorkArrangements;
            InitialiseDepartmentComboBox(departmentList);
            MonthGrid_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].allEmployeeWorkArrangement;
            FindEventDaysInMonth(daysInMonth);
            SetPropertiesForDataGrids();
        }

        private void SetPropertiesForDataGrids()
        {
            MonthGrid_DataGrid.CanUserResizeColumns = false;
            MonthGrid_DataGrid.CanUserResizeRows = false;
            MonthGrid_DataGrid.CanUserDeleteRows = false;
            MonthGrid_DataGrid.CanUserSortColumns = false;
            MonthGrid_DataGrid.CanUserAddRows = false;
            MonthGrid_DataGrid.CanUserReorderColumns = false;
            MonthGrid_DataGrid.MinColumnWidth = 30;
            MonthGrid_DataGrid.MinRowHeight = 30;
            MonthGrid_DataGrid.AutoGenerateColumns = false;
            MonthGrid_DataGrid.AlternatingRowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
            MonthGrid_DataGrid.Columns[0].CellStyle = MonthGrid_DataGrid.TryFindResource("BoldNameStyle") as Style;
            MonthGrid_DataGrid.Columns[0].IsReadOnly = true;
            MonthGrid_DataGrid.Columns[0].Width = 250;
            MonthGrid_DataGrid.SelectionMode = DataGridSelectionMode.Single;
            MonthGrid_DataGrid.IsReadOnly = true;

            foreach (int day in eventDays)
            {
                MonthGrid_DataGrid.Columns[day].CellStyle = MonthGrid_DataGrid.TryFindResource("WeekendStyle") as Style;
            }            
        }

        private void FindEventDaysInMonth(int daysInMonth)
        {
            eventDays = EventDay.GetEventDaysFromDBasDaysNumbers(choosenMonth, choosenYear);

            DateTime dt = new DateTime(choosenYear, choosenMonth, 1);
            Debug.Write(dt);
            for (int i = 0; i < daysInMonth; i++)
            {
                Debug.Write(dt.DayOfWeek);
                if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (!eventDays.Contains(i + 1))
                    {
                        eventDays.Add(i + 1);
                    }
                }
                dt = dt.AddDays(1);
            }
            workingHoures = CalculateWorkingHoures(eventDays.Count, daysInMonth, MonthView.FullTime);
            workingHoures_TextBox.Text = RaportData.convertWorkingHoures(workingHoures).Replace("(", "\n(");
            return;
        }

        private void GetNumberOfDaysInMonth()
        {
            var splitedValuesFromComboBox = ChoosenMonth_ComboBox.Text.Split(" ");
            string month = splitedValuesFromComboBox[0];

            int monthNumber = 0;
            int year = int.Parse(splitedValuesFromComboBox[1]);
            choosenYear = year;

            // finding which month was in combobox
            for (int i = 0; i < 12; i++)
            {
                if (month.Equals(App.months[i]))
                {
                    monthNumber = i + 1;
                    choosenMonth = monthNumber;
                    break;
                }
            }
            daysInMonth = DateTime.DaysInMonth(year, monthNumber);
            return;
        }

        private void MonthChoosed_Click(object sender, RoutedEventArgs e)
        {
            GetNumberOfDaysInMonth();

            string path;
            if (isUserEnteringRequests)
            {
                path = System.IO.Path.Combine(App.folderPath, "Requests");
            }
            else
            {
                path = System.IO.Path.Combine(App.folderPath, "Schedules");
            }

            var requestFiles = Directory.EnumerateFiles(path, "*.txt");
            if (requestFiles.Any())
            {
                bool requestForChoosenMonthExist = false;
                string fileToCompare = choosenMonth.ToString() + "_" + choosenYear.ToString() + "_grafik.txt";
                foreach (string file in requestFiles)
                {
                    string shortFileName = file.Replace(path + "\\", "");             
                    if (shortFileName.Equals(fileToCompare))
                    {
                        requestForChoosenMonthExist = true;
                    }
                }
                if (requestForChoosenMonthExist)
                {
                    string messageBoxText;

                    if (isUserEnteringRequests)
                    {
                        messageBoxText = "Prośby na: " + App.months[choosenMonth - 1] + " " + choosenYear.ToString() + " już istnieją \n" +
                                         "Otworzone zostaną ostatnio wprowadzone prośby";
                    }
                    else
                    {
                        messageBoxText = "Grafik na: " + App.months[choosenMonth - 1] + " " + choosenYear.ToString() + " już istnieje \n" +
                                         "Otworzony zostanie ostatnio utworzpny grafik";
                    }
                    string caption = "Harmonogram";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Information;
                    MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);

                    // part for getting data from file
                    string filePath;
                    if (isUserEnteringRequests)
                    {
                        filePath = "Requests\\" + fileToCompare;
                    }
                    else
                    {
                        filePath = "Schedules\\" + fileToCompare;
                    }
                    ScheduleData data = ScheduleFile.ReadExistingSchedule(filePath);
                    GenerateNewMonthView(data);
                }
                else
                {
                    GenerateNewMonthView();
                }
            }
            else
            {
                Debug.WriteLine("No requests at all");
                GenerateNewMonthView();
            }

            MonthChoosed_Button.IsEnabled = false;
            GenerateSchedule_Button.IsEnabled = true;
            GenerateRaport_Button.IsEnabled = true;
            Department_ComboBox.IsEnabled = true;
            SaveSchedule_Button.IsEnabled = true;
        }

        private void ShowDepartment_Click(object sender, RoutedEventArgs e)
        {
            MonthGrid_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].allEmployeeWorkArrangement;
            ShowDepartment_Button.IsEnabled = false;
        }

        private void Department_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowDepartment_Button.IsEnabled = true;
        }

        private void ChoosenMonth_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MonthChoosed_Button.IsEnabled = true;
        }

        private void GenerateRaport_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < departmentsWorkArrangement.Count; i++)
            {
                RaportData raportData = new RaportData(
                    departmentsWorkArrangement[i], 
                    daysInMonth, 
                    choosenMonth, 
                    choosenYear,
                    departmentList[i],
                    workingHoures,
                    eventDays
                );
                Raport raport = new Raport(raportData);
                raport.GenerateRaport();
                raport.SaveRaport();
            }
            string messageBoxText = "Raport został wygenerowany.";
            string caption = "Raport";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
        }

        private void MonthGrid_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int nurseIndex = MonthGrid_DataGrid.SelectedIndex;
            int dayInMonth = MonthGrid_DataGrid.CurrentCell.Column.DisplayIndex;
            EmployeeWorkArrangement selectedEmployeeWorkArrangemnent = (EmployeeWorkArrangement)MonthGrid_DataGrid.SelectedItem;

            if (MonthGrid_DataGrid.SelectedItem != null)
            {
                MonthGrid_DataGrid.SelectedItem = null;
                if (!forbiddenClickRows.Contains(nurseIndex))
                {
                    bool cntrlKeyDown = Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl);
                    string nextShitfType = NextShiftType(selectedEmployeeWorkArrangemnent.GetSingleWorkArrangement(dayInMonth), cntrlKeyDown);
                    departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, nextShitfType);
                }
                MonthGrid_DataGrid.Items.Refresh();
            }  
        }

        private string NextShiftType(string currentShift, bool isCtrlDown)
        {
            string toRetuurn = "";
            if (isCtrlDown)
            {
                switch (currentShift)
                {
                    default:
                        toRetuurn = "D";
                        break;
                    case "D":
                        toRetuurn = "N";
                        break;
                    case "N":
                        toRetuurn = "/";
                        break;
                    case "/":
                        toRetuurn = "r";
                        break;
                    case "r":
                        toRetuurn = "p";
                        break;
                    case "p":
                        toRetuurn = "U";
                        break;
                    case "U":
                        toRetuurn = "Um";
                        break;
                    case "Um":
                        toRetuurn = "Us";
                        break;
                    case "Us":
                        toRetuurn = "Uo";
                        break;
                    case "Uo":
                        toRetuurn = "Uż";
                        break;
                    case "Uż":
                        toRetuurn = "C";
                        break;
                    case "C":
                        toRetuurn = "Op";
                        break;
                    case "Op":
                        toRetuurn = "";
                        break;
                }
            }
            else
            {
                switch (currentShift)
                {
                    default:
                        toRetuurn = "D";
                        break;
                    case "D":
                        toRetuurn = "N";
                        break;
                    case "N":
                        toRetuurn = "/";
                        break;
                    case "/":
                        toRetuurn = "";
                        break;
                }
            }
            return toRetuurn;
        }
        
        private void GenerateSchedule_Click(object sender, RoutedEventArgs e)
        {
            // get chosen department
            Department myDepartment = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].department;
            DepartmentWorkArrangement myDwa;

            // load requests for choosen Month
            string path = System.IO.Path.Combine(App.folderPath, "Requests");        

            var requestFiles = Directory.EnumerateFiles(path, "*.txt");

            if (requestFiles.Any())
            {
                bool requestForChoosenMonthExist = false;
                string fileToCompare = choosenMonth.ToString() + "_" + choosenYear.ToString() + "_grafik.txt";
                foreach (string file in requestFiles)
                {
                    string shortFileName = file.Replace(path + "\\", "");
                    if (shortFileName.Equals(fileToCompare))
                    {
                        requestForChoosenMonthExist = true;
                        break;
                    }
                }
                if (requestForChoosenMonthExist)
                {
                    // part for getting data from file
                    string filePath = "Requests\\" + fileToCompare;
                    
                    ScheduleData data = ScheduleFile.ReadExistingSchedule(filePath);
                    var dwa = data.ListOfDepartmentsWorkArrangements;

                    List<DepartmentWorkArrangement> myDwaList = dwa.Where(d => d.department.Id.Equals(myDepartment.Id)).ToList();

                    if (myDwaList.Any())
                    {
                        myDwa = myDwaList.First();
                    }
                    else
                    {
                        Debug.WriteLine("Didn't find request for this month. Generating empty data");
                        myDwa = null;
                    }
                }
                else
                {
                    Debug.WriteLine("Didn't find request for this month. Generating empty data");
                    myDwa = null;
                }
            }
            else
            {
                Debug.WriteLine("No requests at all. Generating empty data.");
                myDwa = null;
            }

            if (myDwa == null)
            {

            }

            Solver test = new Solver(departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]], eventDays, choosenYear, choosenMonth, myDwa);
            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]] = test.getSchedule();
            MonthGrid_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].allEmployeeWorkArrangement;




        }

        private async void SaveSchedule_Click(object sender, RoutedEventArgs e)
        {
            ScheduleData scheduleData = new ScheduleData(choosenMonth, choosenYear, workingHoures, departmentsWorkArrangement);
            ScheduleFile scheduleFile = new ScheduleFile(scheduleData);
            await scheduleFile.Save(!isUserEnteringRequests); // requests - false, schedule - true
        }

        public static double CalculateWorkingHoures(int eventDaysNumber, int daysInMonth, int workingTime)
        {
            if (workingTime == MonthView.FullTime)
            {
                int workingDays = daysInMonth - eventDaysNumber;
                double workingHoures = workingDays * 455.0 / 60.0;
                Debug.WriteLine("\neventDaysNumber: " + eventDaysNumber.ToString());
                Debug.WriteLine("daysInMonth: " + daysInMonth.ToString());
                Debug.WriteLine("Working houres: " + workingHoures.ToString());
                return workingHoures;
            }

            if (workingTime == MonthView.HalfTime)
            {
                int workingDays = daysInMonth - eventDaysNumber;
                double workingHoures = workingDays * 455.0 / 60.0 / 2.0;
                Debug.WriteLine("\neventDaysNumber: " + eventDaysNumber.ToString());
                Debug.WriteLine("daysInMonth: " + daysInMonth.ToString());
                Debug.WriteLine("Working houres: " + workingHoures.ToString());
                return workingHoures;
            }

            if (workingTime == MonthView.ThreeFourthOfTime)
            {
                int workingDays = daysInMonth - eventDaysNumber;
                double workingHoures = workingDays * 455.0 / 60.0 * 3.0 / 4.0;
                Debug.WriteLine("\neventDaysNumber: " + eventDaysNumber.ToString());
                Debug.WriteLine("daysInMonth: " + daysInMonth.ToString());
                Debug.WriteLine("Working houres: " + workingHoures.ToString());
                return workingHoures;
            }

            return 0.0;
        }
    }
}
