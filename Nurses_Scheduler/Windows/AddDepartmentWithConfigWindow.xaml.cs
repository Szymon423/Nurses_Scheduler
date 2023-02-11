﻿using Nurses_Scheduler.Classes.DataBaseClasses;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for AddDepartmentWithConfig.xaml
    /// </summary>
    public partial class AddDepartmentWithConfigWindow : Window
    {
        private Department department;

        private List<FundamentalEmployee> fundamentalEmployees;
        private List<ComplementaryEmployee> complementaryEmployees;


        public AddDepartmentWithConfigWindow()
        {
            InitializeComponent();

            department= new Department();
            fundamentalEmployees = new List<FundamentalEmployee>();
            complementaryEmployees = new List<ComplementaryEmployee>();

            ComplementaryEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            ComplementaryEmployeeOccupation_ComboBox.SelectedIndex = 0;

            FundamentalEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            FundamentalEmployeeOccupation_ComboBox.SelectedIndex = 0;

            TotalMinEmployeesNumber_TextBox.Text = "0";
            MinFundamentalEmployeesNumber_TextBox.Text = "0";

            Update_Button.IsEnabled = false;
            Update_Button.Visibility = Visibility.Hidden;
        
            Delete_Button.IsEnabled = false;
            Delete_Button.Visibility = Visibility.Hidden;
        }

        public AddDepartmentWithConfigWindow(Department dep)
        {
            InitializeComponent();

            department = dep;
            fundamentalEmployees = FundamentalEmployee.GetFundamentalEmployeesDB(department.Id);

            ComplementaryEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            ComplementaryEmployeeOccupation_ComboBox.SelectedIndex = 0;

            FundamentalEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            FundamentalEmployeeOccupation_ComboBox.SelectedIndex = 0;

            TotalMinEmployeesNumber_TextBox.Text = department.MinimalEmployeePerDayShift.ToString();
            MinFundamentalEmployeesNumber_TextBox.Text = "0";

            DepartmentFullName_TextBox.Text = department.DepartmentName;
            DepartmentShortName_TextBox.Text = department.DepartmentShortName;

            AddDepartment_Button.IsEnabled = false;
            AddDepartment_Button.Visibility = Visibility.Hidden;

            FundamentalEmployees_ListView.ItemsSource = FundamentalEmployee.GetFundamentalEmployeesDB(department.Id, "D");
            ComplementaryEmployees_ListView.ItemsSource = ComplementaryEmployee.GetComplementaryEmployeesDB(department.Id, "D");

        }

        private void AddDepartment_Button_Click(object sender, RoutedEventArgs e)
        {
            department.DepartmentName = DepartmentFullName_TextBox.Text;
            department.DepartmentShortName = DepartmentShortName_TextBox.Text;
            department.MinimalEmployeePerDayShift = Int32.Parse(TotalMinEmployeesNumber_TextBox.Text);
            // do poprawy na odpowiednią zmienną
            department.MinimalEmployeePerNightShift = Int32.Parse(TotalMinEmployeesNumber_TextBox.Text);
            
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Department>();
                connection.Insert(department);

                int Id = connection.Table<Department>().OrderBy(d => d.Id).Last().Id;

                foreach (FundamentalEmployee fundamentalEmployee in fundamentalEmployees)
                {
                    fundamentalEmployee.DepartmentId = Id;
                    connection.CreateTable<FundamentalEmployee>();
                    connection.Insert(fundamentalEmployee);
                }
            }
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void IncreaseTotalEmployeesNumber_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(TotalMinEmployeesNumber_TextBox.Text);
            TotalMinEmployeesNumber_TextBox.Text = (val + 1).ToString();
        }

        private void DecreaseTotalEmployeesNumber_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(TotalMinEmployeesNumber_TextBox.Text);
            if (val > 0)
            {
                TotalMinEmployeesNumber_TextBox.Text = (val - 1).ToString();
            }
        }

        private void AddFundamentalConfiguration_Button_Click(object sender, RoutedEventArgs e)
        {
            FundamentalEmployee fundamentalEmployee = new FundamentalEmployee()
            {
                ShiftType = "D",
                DepartmentId = department.Id,
                Occupation = FundamentalEmployeeOccupation_ComboBox.Text,
                MinEmployeesNumber = Int32.Parse(MinFundamentalEmployeesNumber_TextBox.Text)
            };
            fundamentalEmployees.Add(fundamentalEmployee);
            FundamentalEmployees_ListView.ItemsSource = fundamentalEmployees;
            FundamentalEmployees_ListView.Items.Refresh();
        }

        private void AddComplementaryConfiguration_Button_Click(object sender, RoutedEventArgs e)
        {
            ComplementaryEmployee complementaryEmployee = new ComplementaryEmployee()
            {
                ShiftType = "D",
                DepartmentId = department.Id,
                Occupation = ComplementaryEmployeeOccupation_ComboBox.Text
            };
            complementaryEmployees.Add(complementaryEmployee);
            ComplementaryEmployees_ListView.ItemsSource = complementaryEmployees;
            ComplementaryEmployees_ListView.Items.Refresh();
        }

        private void DecreaseFundamentalEmployeesNumber_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(MinFundamentalEmployeesNumber_TextBox.Text);
            if (val > 0)
            {
                MinFundamentalEmployeesNumber_TextBox.Text = (val - 1).ToString();
            }
        }

        private void IncreaseFundamentalEmployeesNumber_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(MinFundamentalEmployeesNumber_TextBox.Text);
            MinFundamentalEmployeesNumber_TextBox.Text = (val + 1).ToString();
        }

        private void Delte_Button_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Department>();
                connection.Delete(department);
            }
            Close();
        }

        private void Update_Button_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
