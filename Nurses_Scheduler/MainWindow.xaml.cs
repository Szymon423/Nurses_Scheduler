﻿using Nurses_Scheduler.Classes.DataBaseClasses;
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
using System.IO;
using System.Diagnostics;

namespace Nurses_Scheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Department> departmentList;
        List<Employee> employeeList;
        List<EventDay> eventDays;

        public MainWindow()
        {
            InitializeComponent();

            departmentList = new List<Department>();
            employeeList = new List<Employee>();
            eventDays = new List<EventDay>();

            ReadDatabase();

            Directory.CreateDirectory("Requests");
            Directory.CreateDirectory("Schedules");
        }

        private void AddDepartmentWindow_Button(object sender, RoutedEventArgs e)
        {
            AddDepartmentWithConfigWindow addDepartmentWithConfigWindow = new AddDepartmentWithConfigWindow();
            addDepartmentWithConfigWindow.ShowDialog();
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

                conn.CreateTable<EventDay>();
                eventDays = conn.Table<EventDay>().ToList();
            }

            if (departmentList != null)
            {
                App.DepartmentIdToName.Clear();
                foreach (Department department in departmentList)
                {
                    App.DepartmentIdToName.Add(department.Id, department.DepartmentShortName);
                }
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
                AddDepartmentWithConfigWindow addDepartmentWithConfigWindow = new AddDepartmentWithConfigWindow(selectedDepartment);
                addDepartmentWithConfigWindow.ShowDialog();
            }
            ReadDatabase();
        }

        private void Employee_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Employee selectedEmployee = (Employee)Employee_ListView.SelectedItem;

            if (selectedEmployee != null)
            {
                EditEmployeeDataOrVacationsWindow test = new EditEmployeeDataOrVacationsWindow(selectedEmployee);
                test.ShowDialog();  
            }
            ReadDatabase();
        }

        private void EnterRequests_Button(object sender, RoutedEventArgs e)
        {
            MonthView monthView = new MonthView(true);
            monthView.ShowDialog();
        }

        private void ModifyExistingSchedule_Button(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateSchedule_Button(object sender, RoutedEventArgs e)
        {
            MonthView monthView = new MonthView(false);
            monthView.ShowDialog();
        }

        private void EventDays_Button(object sender, RoutedEventArgs e)
        {
            EventDaysWindow eventDaysWindow = new EventDaysWindow();
            eventDaysWindow.ShowDialog();
            ReadDatabase();
        }

    }
}
