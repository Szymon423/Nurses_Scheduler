﻿using Nurses_Scheduler.Classes.DataBaseClasses;
using Nurses_Scheduler.Classes.Raport;
using Nurses_Scheduler.Classes.RaportClases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

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

        public ScheduleData()
        {
            this.Month = 0;
            this.Year = 0;
            this.WorkingHours = 0.0;
            this.ListOfDepartmentsWorkArrangements = new List<DepartmentWorkArrangement>();
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
                AddDataToContentFile("d: " + dwa.department.Id.ToString());
                foreach (var ewa in dwa.GetEmployeeWorkArrangementAsListWithEmployeeData())
                {
                    string Line = "";
                    foreach (string part in ewa)
                    {
                        Line += part + ", ";
                    }
                    AddDataToContentFile(Line);
                }
                AddDataToContentFile("");
            }
        }

        private void AddDataToContentFile(string dataToAdd)
        {
            FileContent.Add(dataToAdd);
        }

        public async Task Save(bool requestsOrSchedule) // requests - 0, schedule - 1
        {
            string raportFileName = scheduleData.Month.ToString() + "_" +
                                    scheduleData.Year.ToString() + "_" +
                                    "grafik.txt";

            string path;
            if (requestsOrSchedule) // requests - 0, schedule - 1
            {
                Directory.CreateDirectory("Schedules");
                path = System.IO.Path.Combine(App.folderPath, "Schedules");
            }
            else
            {
                Directory.CreateDirectory("Requests");
                path = System.IO.Path.Combine(App.folderPath, "Requests");

            }

            var requestFiles = Directory.EnumerateFiles(path, "*.txt");
            if (requestFiles.Any())
            {
                bool requestForChoosenMonthExist = false;
                foreach (string file in requestFiles)
                {
                    string shortFileName = file.Replace(path + "\\", "");
                    if (shortFileName.Equals(raportFileName))
                    {
                        requestForChoosenMonthExist = true;
                    }
                }
                if (requestForChoosenMonthExist)
                {
                    string messageBoxText = "Próbujesz nadpisać istniejący zapis? \n" + 
                                            "Stare dane zostaną utracone, czy chcesz to wykonać?";
                    string caption = "Zapisywanie";
                    MessageBoxButton button = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Question;
                    var answer = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

                    if (answer == MessageBoxResult.No)
                    {
                        return;
                    }   
                }  
            }
            if (requestsOrSchedule) // requests - 0, schedule - 1
            {
                await File.WriteAllLinesAsync("Schedules\\" + raportFileName, FileContent);
            }
            else
            {
                await File.WriteAllLinesAsync("Requests\\" + raportFileName, FileContent);

            }
            

            string messageBoxText1 = "Grafik został zapisany";
            string caption1 = "Zapisywanie";
            MessageBoxButton button1 = MessageBoxButton.OK;
            MessageBoxImage icon1 = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText1, caption1, button1, icon1, MessageBoxResult.None);
        }

        public static ScheduleData ReadExistingSchedule(string path)
        {
            string[] lines = File.ReadAllLines(path);
            ScheduleData data = new ScheduleData();
            DepartmentWorkArrangement dwa = new DepartmentWorkArrangement(new Department());
            List<Employee> employeesFromDB = new List<Employee>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                Debug.WriteLine(line);

                if      (i == 0) data.Month = Int32.Parse(line);
                else if (i == 1) data.Year = Int32.Parse(line);
                else if (i == 2) data.WorkingHours = float.Parse(line);
                else
                {
                    if (line == "")
                    {
                        continue;
                    }
                    else
                    {
                        if (line.StartsWith("d: "))
                        {
                            if (i != 4)
                            {
                                data.ListOfDepartmentsWorkArrangements.Add(dwa);
                            }
                            int departmentId = int.Parse(line.Substring(3));
                            Department department = Department.GetDepartmentById(departmentId);
                            employeesFromDB = Employee.GetEmployeeByDepartment(department);
                            dwa = new DepartmentWorkArrangement(department); 
                        }
                        else
                        {
                            string[] subLines = line.Split(",");
                            int employeeId = Int32.Parse(subLines[0]);
                            Employee employee = Employee.GetEmployeeById(employeeId);
                            if (employee == null)
                            {
                                continue;
                            }
                            for (int empl_i = 0; empl_i < employeesFromDB.Count; empl_i++)
                            {
                                if (employee.Id == employeesFromDB[empl_i].Id)
                                {
                                    employeesFromDB.RemoveAt(empl_i);
                                    break;
                                }
                            }
                            EmployeeWorkArrangement ewa = new EmployeeWorkArrangement(employee);
                            for (int j = 1; j < subLines.Length - 1; j++)
                            {
                                ewa.SetEmployeeWorkArrangement(j, subLines[j]);
                            }
                            dwa.allEmployeeWorkArrangement.Add(ewa);
                        }

                    }
                }
            }
            foreach (Employee employee in employeesFromDB)
            {
                EmployeeWorkArrangement ewa = new EmployeeWorkArrangement(employee);
                dwa.allEmployeeWorkArrangement.Add(ewa);
            }
            data.ListOfDepartmentsWorkArrangements.Add(dwa);
            return data;
        }
    }
}
