using Nurses_Scheduler.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

        public struct ElementsInSingleRow
        {
            public string header_Pracownik { get; set; }
            public string header_1 { get; set; }
            public string header_2 { get; set; }
            public string header_3 { get; set; }
            public string header_4 { get; set; }
            public string header_5 { get; set; }
            public string header_6 { get; set; }
            public string header_7 { get; set; }
            public string header_8 { get; set; }
            public string header_9 { get; set; }
            public string header_10 { get; set; }
            public string header_11 { get; set; }
            public string header_12 { get; set; }
            public string header_13 { get; set; }
            public string header_14 { get; set; }
            public string header_15 { get; set; }
            public string header_16 { get; set; }
            public string header_17 { get; set; }
            public string header_18 { get; set; }
            public string header_19 { get; set; }
            public string header_20 { get; set; }
            public string header_21 { get; set; }
            public string header_22 { get; set; }
            public string header_23 { get; set; }
            public string header_24 { get; set; }
            public string header_25 { get; set; }
            public string header_26 { get; set; }
            public string header_27 { get; set; }
            public string header_28 { get; set; }
            public string header_29 { get; set; }
            public string header_30 { get; set; }
            public string header_31 { get; set; }
        }


        private void GenerateNewMonthView(int daysInMonth)
        {

            List <string> headers = new List<string> ();
            headers.Add("Pracownik");
            for (int i = 0; i < daysInMonth; i++)
            {
                headers.Add((i + 1).ToString());
            }

            MonthGrid_DataGrid.Columns.Clear();
            MonthGrid_DataGrid.Items.Clear();

            DataGrid d = new DataGrid();
            DataGridTextColumn name = new DataGridTextColumn();

            for (int i = 0; i < headers.Count; i++)
            {
                DataGridTextColumn t = new DataGridTextColumn();
                t.Header = headers[i];
                t.Binding = new Binding("header_" + headers[i]);
                MonthGrid_DataGrid.Columns.Add(t);
            }

            ElementsInSingleRow item = new ElementsInSingleRow
            {
                header_Pracownik = "Testowy pracownik",
                header_1 = "a",
                header_2 = "b",
                header_3 = "c",
                header_4 = "d",
                header_5 = "e",
                header_6 = "a",
                header_7 = "b",
                header_8 = "c",
                header_9 = "d",
                header_10 = "e",
                header_11 = "a",
                header_12 = "b",
                header_13 = "c",
                header_14 = "d",
                header_15 = "e",
                header_16 = "a",
                header_17 = "b",
                header_18 = "c",
                header_19 = "d",
                header_20 = "e",
                header_21 = "a",
                header_22 = "b",
                header_23 = "c",
                header_24 = "d",
                header_25 = "e",
                header_26 = "a",
                header_27 = "b",
                header_28 = "c",
                header_29 = "d",
                header_30 = "d",
                header_31 = "d"
            };

            MonthGrid_DataGrid.Items.Add(item);

        }

        interface INamedObject
        {
            string Name { get; }
        }

    private void MonthChoosed_Click(object sender, RoutedEventArgs e)
        {
            var daysDictionary = new Dictionary<string, int>(){
                {"28 dni", 28},
                {"30 dni", 30},
                {"31 dni", 31},
                {"5 dni", 5}
            };

            if (daysDictionary.ContainsKey(DaysInMonth_ComboBox.Text))
            {
                int daysInMonth = daysDictionary[DaysInMonth_ComboBox.Text]; 
                GenerateNewMonthView(daysInMonth);
            }
        }
    }
}
