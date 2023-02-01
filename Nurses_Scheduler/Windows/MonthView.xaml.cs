using Nurses_Scheduler.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for MonthView.xaml
    /// </summary>
    public partial class MonthView : Window
    {
        

        public MonthView()
        {
            
            InitializeComponent();

            List <Month> list = new List<Month>();

            Month nati = new Month("Natalia Kowal");
            Month szymek = new Month("Szymon Wąchała");
            Month test1 = new Month("pracownik 1");
            Month test2 = new Month("pracownik 2");
            Month test3 = new Month("pracownik 3");
            Month test4 = new Month("pracownik 4");

            list.Add (nati);
            list.Add (szymek);
            list.Add (test1); 
            list.Add (test2);   
            list.Add (test3);
            list.Add (test4);

            MonthGrid_DataGrid.CanUserResizeColumns = false;
            MonthGrid_DataGrid.CanUserResizeRows = false;
            MonthGrid_DataGrid.CanUserDeleteRows = false;
            MonthGrid_DataGrid.CanUserSortColumns = false;
            MonthGrid_DataGrid.CanUserAddRows = false;
            MonthGrid_DataGrid.CanUserReorderColumns = false;
            MonthGrid_DataGrid.MinColumnWidth = 40;
            MonthGrid_DataGrid.MinRowHeight = 40;
            MonthGrid_DataGrid.AutoGenerateColumns = false;

        }


        private void GenerateNewMonthView(int daysInMonth)
        {

            MonthGrid_DataGrid.Columns.Clear();

            DataGrid d = new DataGrid();

            DataGridTextColumn name = new DataGridTextColumn();
            name.Header = "Pracownik";
            MonthGrid_DataGrid.Columns.Add(name);

            for (int i = 0; i < daysInMonth; i++)
            {
                DataGridTextColumn t = new DataGridTextColumn();
                t.Header = (i + 1).ToString();
                MonthGrid_DataGrid.Columns.Add(t);
            }
        }

        private void MonthChoosed_Click(object sender, RoutedEventArgs e)
        {
            var daysDictionary = new Dictionary<string, int>(){
                {"28 dni", 28},
                {"30 dni", 30},
                {"31 dni", 31}
            };

            if (daysDictionary.ContainsKey(DaysInMonth_ComboBox.Text))
            {
                int daysInMonth = daysDictionary[DaysInMonth_ComboBox.Text]; 
                GenerateNewMonthView(daysInMonth);
            }
        }
    }
}
