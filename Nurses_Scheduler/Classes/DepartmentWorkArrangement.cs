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

        public DepartmentWorkArrangement()
        {
            allEmployeeWorkArrangement = new List<EmployeeWorkArrangement>();
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

        public void SetNurseWorkArrangement(int nurseIndex, int dayInMonth, string value)
        {
            allEmployeeWorkArrangement[nurseIndex].SetEmployeeWorkArrangement(dayInMonth, value);
        }
    }
}
