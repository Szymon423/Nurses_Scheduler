﻿using Nurses_Scheduler.Classes.DataBaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes
{
    public class DepartmentWorkArrangement
    {
        public List<EmployeeWorkArrangement> allEmployeeWorkArrangement;
        public Department department;

        public DepartmentWorkArrangement(Department department)
        {
            allEmployeeWorkArrangement = new List<EmployeeWorkArrangement>();
            this.department = department;
        }

        public DepartmentWorkArrangement(DepartmentWorkArrangement dwa)
        {
            this.department = dwa.department;
            this.allEmployeeWorkArrangement = new List<EmployeeWorkArrangement>();
            foreach (EmployeeWorkArrangement e in dwa.allEmployeeWorkArrangement)
            {
                this.allEmployeeWorkArrangement.Add(new EmployeeWorkArrangement(e));
            }
        }

        public List<List<string>> GetEmployeeWorkArrangementAsList()
        {
            List<List<string>> result = new List<List<string>>();
            List<string> headers = new List<string>();
            headers.Add("Pracownik");
            for (int i = 1; i <= 31; i++)
            {
                headers.Add((i).ToString());
            }
            result.Add(headers);
            foreach (EmployeeWorkArrangement emp in allEmployeeWorkArrangement)
            {
                result.Add(emp.GetWorkArrangementAsList());
            }
            return result;
        }

        public List<List<string>> GetEmployeeWorkArrangementAsListWithEmployeeData()
        {
            List<List<string>> result = new List<List<string>>();            
            foreach (EmployeeWorkArrangement emp in allEmployeeWorkArrangement)
            {
                var recievedData = emp.GetWorkArrangementAsListWithEmployeeData();
                if (recievedData != null)
                {
                    result.Add(recievedData);
                }
            }
            return result;
        }

        public void SetNurseWorkArrangement(int nurseIndex, int dayInMonth, string value)
        {
            allEmployeeWorkArrangement[nurseIndex].SetEmployeeWorkArrangement(dayInMonth, value);
        }

        public void CleanAllEmployeeWorkArrangement()
        {
            for (int employee_i = 0; employee_i < allEmployeeWorkArrangement.Count; employee_i++)
            {
                for (int day = 0; day < 31; day++)
                {
                    this.SetNurseWorkArrangement(employee_i, day, "");
                }
            }
        }
    }
}
