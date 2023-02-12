using Nurses_Scheduler.Classes.DataBaseClasses;
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
        private List<FundamentalEmployee> fundamentalEmployees_toDelete;
        private List<FundamentalEmployee> fundamentalEmployees_toAdd;

        private List<ComplementaryEmployee> complementaryEmployees;
        private List<ComplementaryEmployee> complementaryEmployees_toDelete;
        private List<ComplementaryEmployee> complementaryEmployees_toAdd;

        private bool Editing;


        public AddDepartmentWithConfigWindow()
        {
            InitializeComponent();

            department= new Department();

            fundamentalEmployees = new List<FundamentalEmployee>();
            fundamentalEmployees_toAdd = new List<FundamentalEmployee>();
            fundamentalEmployees_toDelete = new List<FundamentalEmployee>();  

            complementaryEmployees = new List<ComplementaryEmployee>();
            complementaryEmployees_toAdd = new List<ComplementaryEmployee>();
            complementaryEmployees_toDelete = new List<ComplementaryEmployee>();     

            Editing = false;

            ComplementaryEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            ComplementaryEmployeeOccupation_ComboBox.SelectedIndex = 0;

            ComplementaryEmployeeOccupation_Night_ComboBox.ItemsSource = App.AllowedOccupations;
            ComplementaryEmployeeOccupation_Night_ComboBox.SelectedIndex = 0;

            FundamentalEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            FundamentalEmployeeOccupation_ComboBox.SelectedIndex = 0;

            FundamentalEmployeeOccupation_Night_ComboBox.ItemsSource = App.AllowedOccupations;
            FundamentalEmployeeOccupation_Night_ComboBox.SelectedIndex = 0;

            TotalMinEmployeesNumber_TextBox.Text = "0";
            TotalMinEmployeesNumber_Night_TextBox.Text = "0";

            MinFundamentalEmployeesNumber_TextBox.Text = "0";
            MinFundamentalEmployeesNumber_Night_TextBox.Text = "0";

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
            fundamentalEmployees_toAdd = new List<FundamentalEmployee>();
            fundamentalEmployees_toDelete = new List<FundamentalEmployee>();

            complementaryEmployees = ComplementaryEmployee.GetComplementaryEmployeesDB(department.Id);
            complementaryEmployees_toAdd = new List<ComplementaryEmployee>();
            complementaryEmployees_toDelete = new List<ComplementaryEmployee>();

            Editing = true;

            ComplementaryEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            ComplementaryEmployeeOccupation_ComboBox.SelectedIndex = 0;

            ComplementaryEmployeeOccupation_Night_ComboBox.ItemsSource = App.AllowedOccupations;
            ComplementaryEmployeeOccupation_Night_ComboBox.SelectedIndex = 0;

            FundamentalEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            FundamentalEmployeeOccupation_ComboBox.SelectedIndex = 0;

            FundamentalEmployeeOccupation_Night_ComboBox.ItemsSource = App.AllowedOccupations;
            FundamentalEmployeeOccupation_Night_ComboBox.SelectedIndex = 0;

            TotalMinEmployeesNumber_TextBox.Text = department.MinimalEmployeePerDayShift.ToString();
            TotalMinEmployeesNumber_Night_TextBox.Text = department.MinimalEmployeePerNightShift.ToString();

            MinFundamentalEmployeesNumber_Night_TextBox.Text = "0";
            MinFundamentalEmployeesNumber_TextBox.Text = "0";

            DepartmentFullName_TextBox.Text = department.DepartmentName;
            DepartmentShortName_TextBox.Text = department.DepartmentShortName;

            AddDepartment_Button.IsEnabled = false;
            AddDepartment_Button.Visibility = Visibility.Hidden;

            FundamentalEmployees_ListView.ItemsSource = fundamentalEmployees.Where(x => x.ShiftType.Equals("D"));
            FundamentalEmployees_Night_ListView.ItemsSource = fundamentalEmployees.Where(x => x.ShiftType.Equals("N"));

            ComplementaryEmployees_ListView.ItemsSource = complementaryEmployees.Where(x => x.ShiftType.Equals("D"));
            ComplementaryEmployees_Night_ListView.ItemsSource = complementaryEmployees.Where(x => x.ShiftType.Equals("N"));
        }

        private void AddDepartment_Button_Click(object sender, RoutedEventArgs e)
        {
            department.DepartmentName = DepartmentFullName_TextBox.Text;
            department.DepartmentShortName = DepartmentShortName_TextBox.Text;
            department.MinimalEmployeePerDayShift = Int32.Parse(TotalMinEmployeesNumber_TextBox.Text);
            department.MinimalEmployeePerNightShift = Int32.Parse(TotalMinEmployeesNumber_Night_TextBox.Text);
            
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

                foreach (ComplementaryEmployee complementaryEmployee in complementaryEmployees)
                {
                    complementaryEmployee.DepartmentId = Id;
                    connection.CreateTable<ComplementaryEmployee>();
                    connection.Insert(complementaryEmployee);
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
            if (Editing)
            {
                fundamentalEmployees_toAdd.Add(fundamentalEmployee);
            }
            FundamentalEmployees_ListView.ItemsSource = fundamentalEmployees.Where(d => d.ShiftType.Equals("D")).ToList(); ;
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
            if (Editing)
            {
                complementaryEmployees_toAdd.Add(complementaryEmployee);
            }
            ComplementaryEmployees_ListView.ItemsSource = complementaryEmployees.Where(d => d.ShiftType.Equals("D")).ToList(); ;
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

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Department>();
                connection.Delete(department);

                connection.CreateTable<FundamentalEmployee>();
                foreach (FundamentalEmployee fundamentalEmployee in fundamentalEmployees)
                {
                    connection.Delete(fundamentalEmployee);
                }

                connection.CreateTable<ComplementaryEmployee>();
                foreach (ComplementaryEmployee complementaryEmployee in complementaryEmployees)
                {
                    connection.Delete(complementaryEmployee);
                }
            }
           Close();
        }

        private void Update_Button_Click(object sender, RoutedEventArgs e)
        {
            department.DepartmentName = DepartmentFullName_TextBox.Text;
            department.DepartmentShortName = DepartmentShortName_TextBox.Text;
            department.MinimalEmployeePerDayShift = Int32.Parse(TotalMinEmployeesNumber_TextBox.Text);
            department.MinimalEmployeePerNightShift = Int32.Parse(TotalMinEmployeesNumber_Night_TextBox.Text);

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Department>();
                connection.Update(department);

                connection.CreateTable<FundamentalEmployee>();
                foreach (FundamentalEmployee employee_toDelete in fundamentalEmployees_toDelete)
                {
                    connection.Delete(employee_toDelete);
                }

                foreach (FundamentalEmployee employee_toAdd in fundamentalEmployees_toAdd)
                {
                    connection.Insert(employee_toAdd);
                }

                connection.CreateTable<ComplementaryEmployee>();
                foreach (ComplementaryEmployee employee_toDelete in complementaryEmployees_toDelete)
                {
                    connection.Delete(employee_toDelete);
                }

                foreach (ComplementaryEmployee employee_toAdd in complementaryEmployees_toAdd)
                {
                    connection.Insert(employee_toAdd);
                }
            }
            Close();
        }

        private void FundamentalEmployees_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FundamentalEmployee selectedEmployee = (FundamentalEmployee)FundamentalEmployees_ListView.SelectedItem;
            
            if (selectedEmployee != null)
            {
                string messageBoxText = "Czy chcesz usunąć Podstawowego pracownika:\n" + 
                                        selectedEmployee.Occupation + "\n" +
                                        "Minimalna ilość pracowników: " + selectedEmployee.MinEmployeesNumber.ToString();
                string caption = "Usuwanie parametru";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Information;
                var resoult = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                if (resoult == MessageBoxResult.Yes)
                {
                    if (Editing)
                    {
                        fundamentalEmployees_toDelete.Add(selectedEmployee);
                    }
                    fundamentalEmployees.Remove(selectedEmployee);
                }
                FundamentalEmployees_ListView.ItemsSource = fundamentalEmployees.Where(d => d.ShiftType.Equals("D")).ToList();
                FundamentalEmployees_ListView.Items.Refresh();
            }
        }

        private void ComplementaryEmployees_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComplementaryEmployee selectedEmployee = (ComplementaryEmployee)ComplementaryEmployees_ListView.SelectedItem;

            if (selectedEmployee != null)
            {
                string messageBoxText = "Czy chcesz usunąć uzupełniającego pracownika:\n" +
                                        selectedEmployee.Occupation;
                string caption = "Usuwanie parametru";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Information;
                var resoult = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                if (resoult == MessageBoxResult.Yes)
                {
                    if (Editing)
                    {
                        complementaryEmployees_toDelete.Add(selectedEmployee);
                    }
                    complementaryEmployees.Remove(selectedEmployee);
                }
                ComplementaryEmployees_ListView.ItemsSource = complementaryEmployees.Where(d => d.ShiftType.Equals("D")).ToList();
                ComplementaryEmployees_ListView.Items.Refresh();
            }
        }

        private void IncreaseTotalEmployeesNumber_Night_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(TotalMinEmployeesNumber_Night_TextBox.Text);
            TotalMinEmployeesNumber_Night_TextBox.Text = (val + 1).ToString();
        }

        private void DecreaseTotalEmployeesNumber_Night_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(TotalMinEmployeesNumber_Night_TextBox.Text);
            if (val > 0)
            {
                TotalMinEmployeesNumber_Night_TextBox.Text = (val - 1).ToString();
            }
        }

        private void ComplementaryEmployees_Night_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComplementaryEmployee selectedEmployee = (ComplementaryEmployee)ComplementaryEmployees_Night_ListView.SelectedItem;

            if (selectedEmployee != null)
            {
                string messageBoxText = "Czy chcesz usunąć uzupełniającego pracownika:\n" +
                                        selectedEmployee.Occupation;
                string caption = "Usuwanie parametru";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Information;
                var resoult = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                if (resoult == MessageBoxResult.Yes)
                {
                    if (Editing)
                    {
                        complementaryEmployees_toDelete.Add(selectedEmployee);
                    }
                    complementaryEmployees.Remove(selectedEmployee);
                }
                ComplementaryEmployees_Night_ListView.ItemsSource = complementaryEmployees.Where(d => d.ShiftType.Equals("N")).ToList();
                ComplementaryEmployees_Night_ListView.Items.Refresh();
            }
        }

        private void AddFundamentalConfiguration_Night_Button_Click(object sender, RoutedEventArgs e)
        {
            FundamentalEmployee fundamentalEmployee = new FundamentalEmployee()
            {
                ShiftType = "N",
                DepartmentId = department.Id,
                Occupation = FundamentalEmployeeOccupation_Night_ComboBox.Text,
                MinEmployeesNumber = Int32.Parse(MinFundamentalEmployeesNumber_Night_TextBox.Text)
            };
            fundamentalEmployees.Add(fundamentalEmployee);
            if (Editing)
            {
                fundamentalEmployees_toAdd.Add(fundamentalEmployee);
            }
            FundamentalEmployees_Night_ListView.ItemsSource = fundamentalEmployees.Where(d => d.ShiftType.Equals("N")).ToList(); ;
            FundamentalEmployees_Night_ListView.Items.Refresh();
        }

        private void DecreaseFundamentalEmployeesNumber_Night_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(MinFundamentalEmployeesNumber_Night_TextBox.Text);
            if (val > 0)
            {
                MinFundamentalEmployeesNumber_Night_TextBox.Text = (val - 1).ToString();
            }
        }

        private void IncreaseFundamentalEmployeesNumber_Night_Button_Click(object sender, RoutedEventArgs e)
        {
            int val = Int32.Parse(MinFundamentalEmployeesNumber_Night_TextBox.Text);
            MinFundamentalEmployeesNumber_Night_TextBox.Text = (val + 1).ToString();
        }

        private void FundamentalEmployees_Night_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FundamentalEmployee selectedEmployee = (FundamentalEmployee)FundamentalEmployees_Night_ListView.SelectedItem;

            if (selectedEmployee != null)
            {
                string messageBoxText = "Czy chcesz usunąć Podstawowego pracownika:\n" +
                                        selectedEmployee.Occupation + "\n" +
                                        "Minimalna ilość pracowników: " + selectedEmployee.MinEmployeesNumber.ToString();
                string caption = "Usuwanie parametru";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Information;
                var resoult = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                if (resoult == MessageBoxResult.Yes)
                {
                    if (Editing)
                    {
                        fundamentalEmployees_toDelete.Add(selectedEmployee);
                    }
                    fundamentalEmployees.Remove(selectedEmployee);
                }
                FundamentalEmployees_Night_ListView.ItemsSource = fundamentalEmployees.Where(d => d.ShiftType.Equals("N")).ToList();
                FundamentalEmployees_Night_ListView.Items.Refresh();
            }
        }

        private void AddComplementaryConfiguration_Night_Button_Click(object sender, RoutedEventArgs e)
        {
            ComplementaryEmployee complementaryEmployee = new ComplementaryEmployee()
            {
                ShiftType = "N",
                DepartmentId = department.Id,
                Occupation = ComplementaryEmployeeOccupation_Night_ComboBox.Text
            };
            complementaryEmployees.Add(complementaryEmployee);
            if (Editing)
            {
                complementaryEmployees_toAdd.Add(complementaryEmployee);
            }
            ComplementaryEmployees_Night_ListView.ItemsSource = complementaryEmployees.Where(d => d.ShiftType.Equals("N")).ToList(); ;
            ComplementaryEmployees_Night_ListView.Items.Refresh();
        }
    }
}
