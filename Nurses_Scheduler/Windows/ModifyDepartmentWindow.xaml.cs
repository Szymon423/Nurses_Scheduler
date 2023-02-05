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
    /// Interaction logic for ModifyDepartmentWindow.xaml
    /// </summary>
    public partial class ModifyDepartmentWindow : Window
    {
        Department department;
        public ModifyDepartmentWindow(Department department)
        {
            InitializeComponent();

            this.department = department;
            DepartmentName_TextBox.Text = department.DepartmentName;
            DepartmentShortName_TextBox.Text = department.DepartmentShortName;
        }

        private void ConfirmChangesDepartment_Button(object sender, RoutedEventArgs e)
        {
            department.DepartmentName = DepartmentName_TextBox.Text;
            department.DepartmentShortName = DepartmentShortName_TextBox.Text;

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Department>();
                connection.Update(department);
            }
            Close();
        }

        private void CancelChangesDepartment_Button(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteDepartment_Button(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Department>();
                connection.Delete(department);
            }
            Close();
        }
    }
}
