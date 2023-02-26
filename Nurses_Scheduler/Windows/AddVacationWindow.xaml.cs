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
    /// Logika interakcji dla klasy AddVacationWindow.xaml
    /// </summary>
    public partial class AddVacationWindow : Window
    {
        private Employee employee;
        
        public AddVacationWindow(Employee _employee)
        {
            employee = _employee;
            InitializeComponent();
            VacationType_ComboBox.ItemsSource = App.vacationTypes.ToList();
            Employee_Label.Content = employee.FullName;
            ReadDataBase();
        }

        private void Vacation_ListView_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            Vacation selectedVacation = (Vacation)Vacations_ListView.SelectedItem;

            if (selectedVacation != null)
            {
                string messageBoxText = "Czy chcesz usunąć wybrany urlop?";
                string caption = "Usuwanie";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Question;
                var answer = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                if (answer == MessageBoxResult.Yes)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                    {
                        connection.CreateTable<Vacation>();
                        connection.Delete(selectedVacation);
                    }
                    ReadDataBase();
                }
            }
        }

        private void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            Vacation vacation = new Vacation()
            {
                EmployeeId = employee.Id,
                startDay = startDay_DatePicker.SelectedDate.Value.Day,
                startMonth = startDay_DatePicker.SelectedDate.Value.Month,
                startYear = startDay_DatePicker.SelectedDate.Value.Year,
                finishDay = finishDay_DatePicker.SelectedDate.Value.Day,
                finishMonth = finishDay_DatePicker.SelectedDate.Value.Month,
                finishYear = finishDay_DatePicker.SelectedDate.Value.Year,
                vacationType = VacationType_ComboBox.Text
            };

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Vacation>();
                connection.Insert(vacation);
            }
            ReadDataBase();
        }

        private void ReadDataBase()
        {
            Vacations_ListView.ItemsSource = Vacation.GetVacationsFromDB(employee.Id);
        }
    }
}
