using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.DataBaseClasses
{
    [Table("Employee")]
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int DepartmentId { get; set; }

        public string Occupation { get; set; }

        public string WorkingTime { get; set; } 

        [Ignore]
        public string FullName
        {
            get
            {
                return LastName + " " + FirstName;
            }
        }

        [Ignore]
        public string DepartmentName
        {
            get
            {
                if (App.DepartmentIdToName.ContainsKey(DepartmentId))
                {
                    return App.DepartmentIdToName[DepartmentId];
                }
                else
                {
                    return "coś poszło nie tak";
                }
            }
        }

        public static List<Employee> GetEmployeesFromDB(string Occupation, int Id)
        {
            List<Employee> employees = new List<Employee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Employee>();
                employees = conn.Table<Employee>().ToList().Where(c => c.Occupation.Contains(Occupation)).Where(c => c.DepartmentId.Equals(Id)).OrderBy(c => c.LastName).ToList();
            }
            return employees;
        }
    }
}
