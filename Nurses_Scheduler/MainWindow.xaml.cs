using Nurses_Scheduler.Classes;
using Nurses_Scheduler.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nurses_Scheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Department> departmentList;
        List<Employee> employeeList;

        public MainWindow()
        {
            InitializeComponent();

            departmentList = new List<Department>();
            employeeList = new List<Employee>();

            ReadDatabase();
        }

        private void AddDepartmentWindow_Button(object sender, RoutedEventArgs e)
        {
            AddDepartmentWindow addDepartmentWindow = new AddDepartmentWindow();
            addDepartmentWindow.ShowDialog();
            ReadDatabase();
        }

        private void AddEmployeeWindow_Button(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();
                addEmployeeWindow.ShowDialog();
                ReadDatabase();
            }
            catch (Exception ex)
            {
                string messageBoxText = "Nie utworzono oddziału, do którego można przypisać pracownika. " +
                                        "Najpierw stwórz oddział, by móc dodać pracownika oraz przypisać go do oddziału.";
                string caption = ex.Message;
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
            } 
        }

        void ReadDatabase()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Department>();
                departmentList = (conn.Table<Department>().ToList()).OrderBy(c => c.DepartmentName).ToList();

                conn.CreateTable<Employee>();
                employeeList = (conn.Table<Employee>().ToList()).OrderBy(c => c.FirstName).ToList();
            }

            if (departmentList != null)
            {
                Department_ListView.ItemsSource = departmentList;
            }
            if (employeeList != null)
            {
                Employee_ListView.ItemsSource = employeeList;
            }
        }


        private void DepartmentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchTextBox = sender as TextBox;

            var filteredList = departmentList.Where(d => d.DepartmentName.ToLower().Contains(searchTextBox.Text.ToLower())).ToList();
            Department_ListView.ItemsSource = filteredList;
        }

        private void EmployeeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchTextBox = sender as TextBox;

            var filteredList = employeeList.Where(d => d.FullName.ToLower().Contains(searchTextBox.Text.ToLower())).ToList();
            Employee_ListView.ItemsSource = filteredList;
        }

        private void Department_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Department selectedDepartment = (Department)Department_ListView.SelectedItem;

            if (selectedDepartment != null)
            {
                ModifyDepartmentWindow modifyDepartmentWindow = new ModifyDepartmentWindow(selectedDepartment);
                modifyDepartmentWindow.ShowDialog();
            }
            ReadDatabase();
        }

        private void Employee_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Employee selectedEmployee = (Employee)Employee_ListView.SelectedItem;

            if (selectedEmployee != null)
            {
                ModifyEmployeeWindow modifyDepartmentWindow = new ModifyEmployeeWindow(selectedEmployee);
                modifyDepartmentWindow.ShowDialog();
            }
            ReadDatabase();
        }

        private void ShowMonthWindow_Button(object sender, RoutedEventArgs e)
        {
            MonthView monthView = new MonthView();

            monthView.ShowDialog();
        }
    }
}
