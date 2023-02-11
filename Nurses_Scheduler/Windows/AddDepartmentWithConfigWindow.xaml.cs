using Nurses_Scheduler.Classes.DataBaseClasses;
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
    /// Interaction logic for AddDepartmentWithConfig.xaml
    /// </summary>
    public partial class AddDepartmentWithConfigWindow : Window
    {
        public AddDepartmentWithConfigWindow()
        {
            InitializeComponent();


            ComplementaryEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            ComplementaryEmployeeOccupation_ComboBox.SelectedIndex = 0;
            FundamentalEmployeeOccupation_ComboBox.ItemsSource = App.AllowedOccupations;
            FundamentalEmployeeOccupation_ComboBox.SelectedIndex = 0;
            TotalMinEmployeesNumber_TextBox.Text = "0";
            MinFundamentalEmployeesNumber_TextBox.Text = "0";
        }

        private void AddDepartment_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {

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
                Department_ = DepartmentFullName_TextBox.Text,
                Occupation = FundamentalEmployeeOccupation_ComboBox.Text,
                MinEmployeesNumber = Int32.Parse(MinFundamentalEmployeesNumber_TextBox.Text)
            };

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<FundamentalEmployee>();
                connection.Insert(fundamentalEmployee);
            }
            FundamentalEmployees_ListView.ItemsSource = FundamentalEmployee.GetFundamentalEmployeesDB(DepartmentFullName_TextBox.Text, "D");
        }

        private void AddComplementaryConfiguration_Button_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
