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
        public int Month;
        public int Year;
        public double WorkingHours;
        public List<DepartmentWorkArrangement> ListOfDepartmentsWorkArrangements;
        
        public ScheduleData(int month, int year, double workingHours, List<DepartmentWorkArrangement> ListOfDepartmentsWorkArrangements)
        {
            this.Month = month;
            this.Year = year;
            this.WorkingHours = workingHours;
            this.ListOfDepartmentsWorkArrangements = new List<DepartmentWorkArrangement>(ListOfDepartmentsWorkArrangements);
        }
    }
    
    public class ScheduleFile
    {
        private ScheduleData scheduleData;
        private List<string> FileContent;
        public ScheduleFile(ScheduleData scheduleData)
        {
            this.scheduleData = scheduleData;
            FileContent = new List<string>();
            WriteAllDataToContentFile();
        }

        private void WriteAllDataToContentFile()
        {
            AddDataToContentFile(scheduleData.Month.ToString());
            AddDataToContentFile(scheduleData.Year.ToString());
            AddDataToContentFile(scheduleData.WorkingHours.ToString("0.00"));

            foreach (DepartmentWorkArrangement dwa in scheduleData.ListOfDepartmentsWorkArrangements)
            {
                AddDataToContentFile("");
                AddDataToContentFile(dwa.department.ToString());
                foreach (var ewa in dwa.GetEmployeeWorkArrangementAsList())
                {
                    string Line = "";
                    foreach (string part in ewa)
                    {
                        Line += part + ", ";
                    }
                    AddDataToContentFile(Line);
                }
            }
        }

        private void AddDataToContentFile(string dataToAdd)
        {
            FileContent.Add(dataToAdd);
        }





        public async Task Save()
        {
            string raportFileName = scheduleData.Month.ToString() + "_" +
                                    scheduleData.Year.ToString() + "_" +
                                    "grafik";


            Directory.CreateDirectory("Schedules");

            await File.WriteAllLinesAsync("Schedules\\" + raportFileName + ".txt", FileContent);
        }


        public static ScheduleData ReadExistingSchedule()
        {
            ScheduleData test = new ScheduleData();
            return test;
        }
    }
}
