using Nurses_Scheduler.Classes;
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
    /// Interaction logic for AddDepartmentWindow.xaml
    /// </summary>
    public partial class AddDepartmentWindow : Window
    {
        public AddDepartmentWindow()
        {
            InitializeComponent();
        }

        private void AddDepartment_Button(object sender, RoutedEventArgs e)
        {
            Department department = new Department()
            { 
                DepartmentName = DepartmentName_TextBox.Text,
                DepartmentShortName = DepartmentShortName_TextBox.Text
            };

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Department>();
                connection.Insert(department);
            }

            Close();
        }
    }
}
