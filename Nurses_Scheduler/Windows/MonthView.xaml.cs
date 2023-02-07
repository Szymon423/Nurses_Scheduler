using Nurses_Scheduler.Classes;
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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
        private int currentMonth;
        private int currentYear;
        private int howManyMonthToShow;
        private List<String> monthsToChoose;

        private int choosenMonth;
        private int choosenYear;
        private int daysInMonth;
        private List<int> eventDays;


        private IDictionary<string, int> OccupationToIndex = new Dictionary<string, int>();
        private IDictionary<string, int> DepartmentToIndex = new Dictionary<string, int>();
        private List<Department> departmentList;
        private List<DepartmentWorkArrangement> departmentsWorkArrangement;


        public MonthView()
        {
            InitializeComponent();
            currentMonth = DateTime.Now.Month;
            currentYear = DateTime.Now.Year;
            choosenMonth = currentMonth;
            choosenYear = currentYear;
            eventDays = new List<int>();
            howManyMonthToShow = 3;
            monthsToChoose = new List<string>();

            departmentList = Department.GetDepartmentsFromDB();
            for (int i = 0; i < departmentList.Count; i++)
            {
                DepartmentToIndex.Add(departmentList[i].DepartmentName, i);
            }

            for (int i = 0; i < App.AllowedOccupations.Length; i++)
            {
                OccupationToIndex.Add(App.AllowedOccupations[i], i);
            }

            departmentsWorkArrangement = new List<DepartmentWorkArrangement>();

            InitialiseMonthComboBox();
            InitialiseDepartmentComboBox();
            ShowDepartment_Button.IsEnabled = false;
        }

        private void InitialiseDepartmentComboBox()
        {
            List<String> departmentToChoose = new List<string>();
            foreach (Department department in departmentList)
            {
                departmentToChoose.Add(department.DepartmentName);
            }
            Department_ComboBox.ItemsSource = departmentToChoose;
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
            MonthGrid_Pielegniarki_DataGrid.ItemsSource = null;
            MonthGrid_Pielegniarki_DataGrid.Columns.Clear();

            MonthGrid_Pozostali_DataGrid.ItemsSource = null;
            MonthGrid_Pozostali_DataGrid.Columns.Clear();

            // make proper collumns basing on prevoiusly made headers list
            for (int i = 0; i < headers.Count; i++)
            {
                DataGridTextColumn t1 = new DataGridTextColumn();
                DataGridTextColumn t2 = new DataGridTextColumn();
                t1.Header = headers[i];
                t2.Header = headers[i];
                t1.Binding = new Binding("_" + headers[i]);
                t2.Binding = new Binding("_" + headers[i]);

                MonthGrid_Pielegniarki_DataGrid.Columns.Add(t1);
                MonthGrid_Pozostali_DataGrid.Columns.Add(t2);
            }
 
            foreach (Department department in departmentList)
            {
                DepartmentWorkArrangement departmentWorkArrangement = new DepartmentWorkArrangement();
                foreach (String occupation in App.AllowedOccupations)
                {
                    List<Employee> employeeList = Employee.GetEmployeesFromDB(occupation, department.DepartmentName);
                    List<EmployeeWorkArrangement> employeeWorkArrangement = new List<EmployeeWorkArrangement>();
                    foreach (Employee employee in employeeList)
                    {
                        if (occupation == "Pielęgniarka" || occupation == "Asystentka Pielęgniarki")
                        {
                            departmentWorkArrangement.nursesWorkArrangement.Add(new EmployeeWorkArrangement(employee.FullName));
                        }
                        else
                        {
                            departmentWorkArrangement.otherThanNursesWorkArrangement.Add(new EmployeeWorkArrangement(employee.FullName));
                        }
                        departmentWorkArrangement.allEmployeeWorkArrangement.Add(new EmployeeWorkArrangement(employee.FullName));
                    }
                }  
                departmentsWorkArrangement.Add(departmentWorkArrangement);
            }

            MonthGrid_Pielegniarki_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].nursesWorkArrangement;
            MonthGrid_Pozostali_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].otherThanNursesWorkArrangement;

            FindEventDaysInMonth(daysInMonth);
            SetPropertiesForDataGrids();
        }

        private void SetPropertiesForDataGrids()
        {
            MonthGrid_Pielegniarki_DataGrid.CanUserResizeColumns = false;
            MonthGrid_Pielegniarki_DataGrid.CanUserResizeRows = false;
            MonthGrid_Pielegniarki_DataGrid.CanUserDeleteRows = false;
            MonthGrid_Pielegniarki_DataGrid.CanUserSortColumns = false;
            MonthGrid_Pielegniarki_DataGrid.CanUserAddRows = false;
            MonthGrid_Pielegniarki_DataGrid.CanUserReorderColumns = false;
            MonthGrid_Pielegniarki_DataGrid.MinColumnWidth = 40;
            MonthGrid_Pielegniarki_DataGrid.MinRowHeight = 30;
            MonthGrid_Pielegniarki_DataGrid.AutoGenerateColumns = false;
            MonthGrid_Pielegniarki_DataGrid.AlternatingRowBackground = new SolidColorBrush(Colors.AliceBlue);
            MonthGrid_Pielegniarki_DataGrid.Columns[0].CellStyle = MonthGrid_Pielegniarki_DataGrid.TryFindResource("BoldNameStyle") as Style;
            MonthGrid_Pielegniarki_DataGrid.Columns[0].IsReadOnly = true;
            MonthGrid_Pielegniarki_DataGrid.Columns[0].Width = 250;
            MonthGrid_Pielegniarki_DataGrid.SelectionMode = DataGridSelectionMode.Single;
            MonthGrid_Pielegniarki_DataGrid.IsReadOnly = true;

            MonthGrid_Pozostali_DataGrid.CanUserResizeColumns = false;
            MonthGrid_Pozostali_DataGrid.CanUserResizeRows = false;
            MonthGrid_Pozostali_DataGrid.CanUserDeleteRows = false;
            MonthGrid_Pozostali_DataGrid.CanUserSortColumns = false;
            MonthGrid_Pozostali_DataGrid.CanUserAddRows = false;
            MonthGrid_Pozostali_DataGrid.CanUserReorderColumns = false;
            MonthGrid_Pozostali_DataGrid.MinColumnWidth = 40;
            MonthGrid_Pozostali_DataGrid.MinRowHeight = 30;
            MonthGrid_Pozostali_DataGrid.AutoGenerateColumns = false;
            MonthGrid_Pozostali_DataGrid.AlternatingRowBackground = new SolidColorBrush(Colors.AliceBlue);
            MonthGrid_Pozostali_DataGrid.Columns[0].CellStyle = MonthGrid_Pozostali_DataGrid.TryFindResource("BoldNameStyle") as Style;
            MonthGrid_Pozostali_DataGrid.Columns[0].IsReadOnly = true;
            MonthGrid_Pozostali_DataGrid.Columns[0].Width = 250;
            MonthGrid_Pozostali_DataGrid.SelectionMode = DataGridSelectionMode.Single;
            MonthGrid_Pozostali_DataGrid.IsReadOnly = true;

            foreach (int day in eventDays)
            {
                Debug.Write(day.ToString() + ", "); 
                MonthGrid_Pozostali_DataGrid.Columns[day].CellStyle = MonthGrid_Pozostali_DataGrid.TryFindResource("WeekendStyle") as Style;
                MonthGrid_Pielegniarki_DataGrid.Columns[day].CellStyle = MonthGrid_Pielegniarki_DataGrid.TryFindResource("WeekendStyle") as Style;
            }            
        }

        private void FindEventDaysInMonth(int daysInMonth)
        {
            eventDays = new List<int>();

            DateTime dt = new DateTime(choosenYear, choosenMonth, 1);
            Debug.Write(dt);
            for (int i = 0; i < daysInMonth; i++)
            {
                Debug.Write(dt.DayOfWeek);
                if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    eventDays.Add(i + 1);
                }
                dt = dt.AddDays(1);
            }
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
            GenerateNewMonthView();
            MonthChoosed_Button.IsEnabled = false;
        }

        private void ShowDepartment_Click(object sender, RoutedEventArgs e)
        {
            MonthGrid_Pielegniarki_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].nursesWorkArrangement;
            MonthGrid_Pozostali_DataGrid.ItemsSource = departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].otherThanNursesWorkArrangement;
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
                    159.15,
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

        private void MonthGrid_Pielegniarki_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int nurseIndex = MonthGrid_Pielegniarki_DataGrid.SelectedIndex;
            int dayInMonth = MonthGrid_Pielegniarki_DataGrid.CurrentCell.Column.DisplayIndex;
            EmployeeWorkArrangement selectedEmployeeWorkArrangemnent = (EmployeeWorkArrangement)MonthGrid_Pielegniarki_DataGrid.SelectedItem;

            if (MonthGrid_Pielegniarki_DataGrid.SelectedItem != null)
            {
                MonthGrid_Pielegniarki_DataGrid.SelectedItem = null;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    switch (selectedEmployeeWorkArrangemnent.GetSingleWorkArrangement(dayInMonth))
                    {
                        default:
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "D");
                            break;
                        case "D":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "N");
                            break;
                        case "N":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "r");
                            break;
                        case "r":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "p");
                            break;
                        case "p":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "U");
                            break;
                        case "U":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "Um");
                            break;
                        case "Um":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "Us");
                            break;
                        case "Us":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "Uo");
                            break;
                        case "Uo":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "Uż");
                            break;
                        case "Uż":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "C");
                            break;
                        case "C":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "Op");
                            break;
                        case "Op":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "");
                            break;
                    }
                }
                else
                {
                    switch (selectedEmployeeWorkArrangemnent.GetSingleWorkArrangement(dayInMonth))
                    {
                        default:
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "D");
                            break;
                        case "D":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "N");
                            break;
                        case "N":
                            departmentsWorkArrangement[DepartmentToIndex[Department_ComboBox.Text]].SetNurseWorkArrangement(nurseIndex, dayInMonth, "");
                            break;
                    }
                }
                MonthGrid_Pielegniarki_DataGrid.Items.Refresh();
            }  
        }
    }
}
