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
    /// Logika interakcji dla klasy EditEmployeeDataOrVacationsWindow.xaml
    /// </summary>
    public partial class EditEmployeeDataOrVacationsWindow : Window
    {
        private Employee employee;

        public EditEmployeeDataOrVacationsWindow(Employee _employee)
        {
            employee = _employee;
            InitializeComponent();
        }

        private void ModifyEmployeeData_Button_Click(object sender, RoutedEventArgs e)
        {
            ModifyEmployeeWindow modifyDepartmentWindow = new ModifyEmployeeWindow(employee);
            Close();
            modifyDepartmentWindow.ShowDialog();
        }

        private void EmployeeVacation_Button_Click(object sender, RoutedEventArgs e)
        {
            AddVacationWindow avw = new AddVacationWindow(employee);
            Close(); 
            avw.ShowDialog();
        }
    }
}
