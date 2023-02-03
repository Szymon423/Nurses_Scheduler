using Nurses_Scheduler.Classes;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        public AddEmployeeWindow()
        {
            InitializeComponent();
            List<Department> departmentList = GetDepartmentsFromDB();
            if (departmentList.Count > 0)
            {
                EmployeeDepartment_ComboBox.ItemsSource = departmentList;
                EmployeeDepartment_ComboBox.SelectedIndex = 0;
            }
            else
            {
                throw new Exception("Brak istniejących oddziałów");
                Close();
            }   
        }

        private void AddEmployee_Button(object sender, RoutedEventArgs e)
        {
            Employee employee = new Employee()
            {
                FirstName = EmployeeName_TextBox.Text,
                LastName = EmployeeLastName_TextBox.Text,
                Department = EmployeeDepartment_ComboBox.Text,
                Occupation = EmployeeOccupation_ComboBox.Text
            };

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Employee>();
                connection.Insert(employee);
            }
            Close();
        }

        private List<Department> GetDepartmentsFromDB()
        {
            List<Department> departments = new List<Department>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Employee>();
                departments = (conn.Table<Department>().ToList()).OrderBy(c => c.DepartmentName).ToList();
            }
            return departments;
        }
    }
}
