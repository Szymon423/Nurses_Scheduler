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
    /// Interaction logic for ModifyEmployeeWindow.xaml
    /// </summary>
    public partial class ModifyEmployeeWindow : Window
    {
        Employee employee;
        public ModifyEmployeeWindow(Employee employee)
        {
            InitializeComponent();

            EmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            EmployeeDepartment_ComboBox.ItemsSource = GetDepartmentsFromDB();
            
            this.employee = employee;
            EmployeeFirstName_TextBox.Text = employee.FirstName;
            EmployeeLastName_TextBox.Text= employee.LastName;
            EmployeeOccupation_ComboBox.Text = employee.Occupation;
            EmployeeDepartment_ComboBox.Text = employee.Department;
        }

        private void ConfirmChangesEmployee_Button(object sender, RoutedEventArgs e)
        {
            employee.FirstName = EmployeeFirstName_TextBox.Text;
            employee.LastName = EmployeeLastName_TextBox.Text;
            employee.Occupation = EmployeeOccupation_ComboBox.Text;
            employee.Department = EmployeeDepartment_ComboBox.Text;

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Employee>();
                connection.Update(employee);
            }
            Close();
        }

        private void CancelChangesEmployee_Button(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteEmployee_Button(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Employee>();
                connection.Delete(employee);
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
