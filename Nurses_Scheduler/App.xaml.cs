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
    }
}
