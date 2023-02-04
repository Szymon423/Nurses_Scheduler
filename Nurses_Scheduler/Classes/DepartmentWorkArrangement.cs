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
        public List<EmployeeWorkArrangement> nursesWorkArrangement;
        public List<EmployeeWorkArrangement> otherThanNursesWorkArrangement;

        public DepartmentWorkArrangement()
        {
            allEmployeeWorkArrangement = new List<EmployeeWorkArrangement>();
            nursesWorkArrangement = new List<EmployeeWorkArrangement>();
            otherThanNursesWorkArrangement = new List<EmployeeWorkArrangement>();
        }
    }
}
