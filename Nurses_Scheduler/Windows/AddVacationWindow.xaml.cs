using Nurses_Scheduler.Classes.DataBaseClasses;
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
            Vacations_ListView.ItemsSource = Vacation.GetVacationsFromDB(employee.Id);
        }

        private void EventDays_ListView_DoubleClicked(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
