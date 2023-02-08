using Nurses_Scheduler.Classes.DataBaseClasses;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Logika interakcji dla klasy EventDaysWindow.xaml
    /// </summary>
    public partial class EventDaysWindow : Window
    {
        public EventDaysWindow()
        {
            InitializeComponent();

            ReadDatabase();
        }

        private void AddEventDay_Button(object sender, RoutedEventArgs e)
        {
            if (Description_TextBox.Text == "" || ChoosedDay_TextBox.Text == "")
            {
                Debug.WriteLine("nie wybrano dnia");
            }
            else
            {
                EventDay eventDay = new EventDay()
                {
                    Day = Int32.Parse(ChoosedDay_TextBox.Text.Substring(0, 2)),
                    Month = Int32.Parse(ChoosedDay_TextBox.Text.Substring(3, 2)),
                    Year = Int32.Parse(ChoosedDay_TextBox.Text.Substring(6, 4)),
                    Description = Description_TextBox.Text,
                    Repeatability = (bool)Repetability_CheckBox.IsChecked.Value
                };

                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    connection.CreateTable<EventDay>();
                    connection.Insert(eventDay);
                }
                ReadDatabase();
            }
        }

        private void Kalendarz_SelectedDatesChanges(object sender, SelectionChangedEventArgs e)
        {
            if (this.Kalendarz.SelectedDate != null && ChoosedDay_TextBox != null) 
            {
                string content = Kalendarz.SelectedDate.ToString().Substring(0, 10);
                Debug.WriteLine(content);
                ChoosedDay_TextBox.Text = content;
            }
        }

        private void EventDays_ListView_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            EventDay eventDay = (EventDay)EventDays_ListView.SelectedItem;
            if (eventDay != null)
            {
                string messageBoxText = "Czy chcesz usunąć dzień:\n" + eventDay.DayInfo + "\n" + eventDay.Description;
                string caption = "Usuwanie elementu";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;
                var result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                    {
                        Debug.WriteLine("kurwa");
                        connection.CreateTable<EventDay>();
                        connection.Delete(eventDay);
                    }
                }
                ReadDatabase();
            }
        }

        void ReadDatabase()
        {
            List<EventDay> eventDays = EventDay.GetEventDaysFromDB();
            
            if (eventDays != null)
            {
                EventDays_ListView.ItemsSource = eventDays;
            }
        }
    }
}
