﻿using Nurses_Scheduler.Classes.DataBaseClasses;
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
            List <Department> departments = Department.GetDepartmentsFromDB();
            EmployeeDepartment_ComboBox.ItemsSource = departments;
            EmployeeDepartment_ComboBox.SelectedItem = departments.Where(c => c.Id.Equals(employee.DepartmentId)).ToList()[0];
            EmployeeWorkTime_ComboBox.ItemsSource = new List<string>() { "Pełny etat", "1/2 etatu", "3/4 etatu" };
            
            this.employee = employee;
            EmployeeFirstName_TextBox.Text = employee.FirstName;
            EmployeeLastName_TextBox.Text= employee.LastName;
            EmployeeOccupation_ComboBox.Text = employee.Occupation;
            EmployeeWorkTime_ComboBox.Text = employee.WorkingTime;
        }

        private void ConfirmChangesEmployee_Button(object sender, RoutedEventArgs e)
        {
            employee.FirstName = EmployeeFirstName_TextBox.Text;
            employee.LastName = EmployeeLastName_TextBox.Text;
            employee.Occupation = EmployeeOccupation_ComboBox.Text;
            employee.DepartmentId = ((Department)EmployeeDepartment_ComboBox.SelectedItem).Id;
            employee.WorkingTime = EmployeeWorkTime_ComboBox.Text;

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
    }
}
