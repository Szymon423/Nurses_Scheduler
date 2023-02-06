using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Nurses_Scheduler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static string databaseName = "data.db";
        static string folderPath = Environment.CurrentDirectory;
        public static string databasePath = System.IO.Path.Combine(folderPath, databaseName);
        public static string[] AllowedOccupations = { "Pielęgniarka" , "Opiekun Medyczny", "Salowa lub Sanitariuszka", "Asystentka Pielęgniarki" };
        public static string[] months = { "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec", "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień" };
        public static string[] shiftTypes = { "D", "N", "r", "p", "U", "Um", "Us", "Uo", "Uż", "C", "Op" };
    }
}
