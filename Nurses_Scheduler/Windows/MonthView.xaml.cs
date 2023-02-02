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

        public MonthView()
        {
            InitializeComponent();
            currentMonth = DateTime.Now.Month;
            currentYear = DateTime.Now.Year;
            howManyMonthToShow = 3;
            monthsToChoose = new List<string>();
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

        private List<Employee> GetEmployeesFromDB()
        {
            List<Employee> employees = new List<Employee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Employee>();
                employees = (conn.Table<Employee>().ToList()).OrderBy(c => c.FirstName).ToList();

                employees = (from c in conn.Table<Employee>().ToList()
                             orderby c.FirstName descending
                             select c).ToList();
            }
            return employees;
        }

        private void GenerateNewMonthView(int daysInMonth)
        {

            List<Employee> employees = GetEmployeesFromDB();

            List <string> headers = new List<string> ();
            headers.Add("Pracownik");
            for (int i = 0; i < daysInMonth; i++)
            {
                headers.Add((i + 1).ToString());
            }
            // clear dataGrid before inserting new month view
            MonthGrid_DataGrid.Columns.Clear();

            DataGrid d = new DataGrid();
            DataGridTextColumn name = new DataGridTextColumn();

            for (int i = 0; i < headers.Count; i++)
            {
                DataGridTextColumn t = new DataGridTextColumn();
                t.Header = headers[i];
                t.Binding = new Binding("_" + headers[i]);
                MonthGrid_DataGrid.Columns.Add(t);
            }

            List<EmployeeWorkArrangement> employeesWorkArrangements = new List<EmployeeWorkArrangement>();
            foreach (Employee empl in employees)
            {
                EmployeeWorkArrangement item = new EmployeeWorkArrangement(empl.FullName);
                employeesWorkArrangements.Add(item);
            }

            MonthGrid_DataGrid.ItemsSource = employeesWorkArrangements;
            MonthGrid_DataGrid.CanUserResizeColumns = false;
            MonthGrid_DataGrid.CanUserResizeRows = false;
            MonthGrid_DataGrid.CanUserDeleteRows = false;
            MonthGrid_DataGrid.CanUserSortColumns = false;
            MonthGrid_DataGrid.CanUserAddRows = false;
            MonthGrid_DataGrid.CanUserReorderColumns = false;
            MonthGrid_DataGrid.MinColumnWidth = 40;
            MonthGrid_DataGrid.MinRowHeight = 40;
            MonthGrid_DataGrid.AutoGenerateColumns = false;
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
            GenerateNewMonthView(GetNumberOfDaysInMonth());
        }
    }
}
