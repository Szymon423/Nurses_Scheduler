using Nurses_Scheduler.Classes;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        private List<String> monthsToChoose;
        private int howManyMonthToShow;
        private string[] months = {"Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec", "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"};
        IDictionary<string, int> OccupationToIndex = new Dictionary<string, int>();
        private List<List<EmployeeWorkArrangement>> employeesWorkArrangements_GroupedByOccupation;
        private List<EmployeeWorkArrangement> employeesWorkArrangements_OtherThanNurse; // opiekuni medyczni, salowe i sanitariuszki
        private List<EmployeeWorkArrangement> employeesWorkArrangements_Nurses;         // pielęgniarki oraz asystentki pielęgniarek

        public MonthView()
        {
            InitializeComponent();
            currentMonth = DateTime.Now.Month;
            currentYear = DateTime.Now.Year;
            howManyMonthToShow = 3;
            monthsToChoose = new List<string>();

            for (int i = 0; i < App.AllowedOccupations.Length; i++)
            {
                OccupationToIndex.Add(App.AllowedOccupations[i], i);
            }

            employeesWorkArrangements_GroupedByOccupation = new List<List<EmployeeWorkArrangement>>();
            employeesWorkArrangements_OtherThanNurse = new List<EmployeeWorkArrangement>();
            employeesWorkArrangements_Nurses = new List<EmployeeWorkArrangement>();
            InitialiseMonthComboBox();
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
                monthsToChoose.Add(months[monthToAdd - 1] + " " + yearToShow.ToString());
            }
            DaysInMonth_ComboBox.ItemsSource = monthsToChoose;
            DaysInMonth_ComboBox.SelectedIndex = 0;
        }

        private List<Employee> GetEmployeesFromDB(string Occupation)
        {
            List<Employee> employees = new List<Employee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Employee>();
                employees = conn.Table<Employee>().ToList().Where(c => c.Occupation.Contains(Occupation)).OrderBy(c => c.LastName).ToList();
            }
            return employees;
        }

        private void GenerateNewMonthView(int daysInMonth)
        {
            employeesWorkArrangements_GroupedByOccupation = new List<List<EmployeeWorkArrangement>>();
            employeesWorkArrangements_OtherThanNurse = new List<EmployeeWorkArrangement>();
            employeesWorkArrangements_Nurses = new List<EmployeeWorkArrangement>();

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

            foreach (string occupation in App.AllowedOccupations)
            {
                List<Employee> employeeList = GetEmployeesFromDB(occupation);
                List<EmployeeWorkArrangement> employeesWorkArrangements = new List<EmployeeWorkArrangement>();

                foreach (Employee employee in employeeList)
                {
                    EmployeeWorkArrangement item = new EmployeeWorkArrangement(employee.FullName);
                    employeesWorkArrangements.Add(item);
                }

                employeesWorkArrangements_GroupedByOccupation.Add(employeesWorkArrangements);

                if (occupation == "Pielęgniarka" || occupation == "Asystentka Pielęgniarki")
                {
                    employeesWorkArrangements_Nurses.AddRange(employeesWorkArrangements);
                }
                else
                {
                    employeesWorkArrangements_OtherThanNurse.AddRange(employeesWorkArrangements);
                }

            }
            MonthGrid_Pielegniarki_DataGrid.ItemsSource = employeesWorkArrangements_Nurses;
            MonthGrid_Pozostali_DataGrid.ItemsSource = employeesWorkArrangements_OtherThanNurse;
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
            MonthGrid_Pielegniarki_DataGrid.MinRowHeight = 40;
            MonthGrid_Pielegniarki_DataGrid.AutoGenerateColumns = false;

            MonthGrid_Pozostali_DataGrid.CanUserResizeColumns = false;
            MonthGrid_Pozostali_DataGrid.CanUserResizeRows = false;
            MonthGrid_Pozostali_DataGrid.CanUserDeleteRows = false;
            MonthGrid_Pozostali_DataGrid.CanUserSortColumns = false;
            MonthGrid_Pozostali_DataGrid.CanUserAddRows = false;
            MonthGrid_Pozostali_DataGrid.CanUserReorderColumns = false;
            MonthGrid_Pozostali_DataGrid.MinColumnWidth = 40;
            MonthGrid_Pozostali_DataGrid.MinRowHeight = 40;
            MonthGrid_Pozostali_DataGrid.AutoGenerateColumns = false;
        }

        private int GetNumberOfDaysInMonth()
        {
            var splitedValuesFromComboBox = DaysInMonth_ComboBox.Text.Split(" ");
            string month = splitedValuesFromComboBox[0];

            int monthNumber = 0;
            int year = int.Parse(splitedValuesFromComboBox[1]);

            // finding which month was in combobox
            for (int i = 0; i < 12; i++)
            {
                if (month.Equals(months[i]))
                {
                    monthNumber = i + 1;
                    break;
                }
            }
            return DateTime.DaysInMonth(year, monthNumber);
        }

        private void MonthChoosed_Click(object sender, RoutedEventArgs e)
        {
            int dni = GetNumberOfDaysInMonth();
            Debug.WriteLine(dni.ToString());
            GenerateNewMonthView(dni);
        }
    }
}
