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
        public AddVacationWindow()
        {
            InitializeComponent();
        }

        private void finishCallendar_SelectedDatesChanges(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EventDays_ListView_DoubleClicked(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
