using Nurses_Scheduler.Classes.Raport;
using Nurses_Scheduler.Classes.RaportClases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Nurses_Scheduler.Classes
{
    public struct ScheduleData
    {
        int Month;
        int Year;
        double WorkingHours;
        List<List<string>> ListOfEmployeeWorkArrangements;
        
        public ScheduleData(int month, int year, double workingHours, List<List<string>> listOfEmployeeWorkArrangements)
        {
            this.Month = month;
            this.Year = year;
            this.WorkingHours = workingHours;
            this.ListOfEmployeeWorkArrangements = new List<List<string>>(listOfEmployeeWorkArrangements);
        }
    }
    
    
    public class ScheduleFile
    {
        private ScheduleData Data;
        private List<string> FileContent;
        public ScheduleFile(int month, int year, double workingHours, List<List<string>> listOfEmployeeWorkArrangements)
        {
            Data = new ScheduleData(month, year, workingHours, listOfEmployeeWorkArrangements); 
        }



        public async Task Save()
        {
            string raportFileName = "someFile";
            await File.WriteAllLinesAsync(raportFileName + ".txt", FileContent);
        }


        public static ScheduleData ReadExistingSchedule()
        {
            // todo
            Debug.WriteLine("dupa");
        }
    }
}
