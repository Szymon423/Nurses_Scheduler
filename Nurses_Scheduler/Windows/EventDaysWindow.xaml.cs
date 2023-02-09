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
            Kalendarz.SelectedDate = DateTime.Now;
            Kalendarz.DisplayDate = DateTime.Now;
            ReadDatabase();
        }

        private void AddEventDay_Button(object sender, RoutedEventArgs e)
        {
            if (ChoosedDay_TextBox.Text == "")
            {
                string messageBoxText = "Nie wybrano żadnego dnia. \nWybierz dzień za pomocą kalendarza by dodać go do systemu.";
                string caption = "Nie wprowadzono wszystkich informacji";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                var result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
            }
            else if (Description_TextBox.Text == "")
            {
                string messageBoxText = "Nie wprowadzono opisu dla wybranego dnia. \nWprowadź opis by dodać wybrany dzień";
                string caption = "Nie wprowadzono wszystkich informacji";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                var result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
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
                ChoosedDay_TextBox.Text = content;
                Description_TextBox.Text = "";
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
