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
    /// Interaction logic for AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        public AddEmployeeWindow()
        {
            InitializeComponent();

            EmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            EmployeeOccupation_ComboBox.SelectedIndex = 0;

            List<Department> departmentList = Department.GetDepartmentsFromDB();
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
                DepartmentId = ((Department)EmployeeDepartment_ComboBox.SelectedItem).Id,
                Occupation = EmployeeOccupation_ComboBox.Text,
                WorkingTime = EmployeeWorkTime_ComboBox.Text                
            };
            
            

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Employee>();
                connection.Insert(employee);
            }
            Close();
        }
    }
}
