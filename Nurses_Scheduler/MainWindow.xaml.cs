using Nurses_Scheduler.Classes;
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

namespace Nurses_Scheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Department> departmentList;

        public MainWindow()
        {
            InitializeComponent();

            departmentList = new List<Department>();

            ReadDatabase();
        }

        private void AddDepartmentWindow_Button(object sender, RoutedEventArgs e)
        {
            AddDepartmentWindow addDepartmentWindow = new AddDepartmentWindow();
            addDepartmentWindow.ShowDialog();
            ReadDatabase();
        }

        private void AddEmployeeWindow_Button(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();  
            addEmployeeWindow.ShowDialog();
            ReadDatabase();
        }

        void ReadDatabase()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Department>();
                departmentList = conn.Table<Department>().ToList();
            }

            if (departmentList != null)
            {
                Department_ListView.ItemsSource = departmentList;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchTextBox = sender as TextBox;

            var filteredList = departmentList.Where(d => d.DepartmentName.ToLower().Contains(searchTextBox.Text.ToLower())).ToList();
            Department_ListView.ItemsSource = filteredList;

        }
    }
}
