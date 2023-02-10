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

        public string Department { get; set; }

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
        public static List<Employee> GetEmployeesFromDB(string Occupation, string DepartmentName)
        {
            List<Employee> employees = new List<Employee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Employee>();
                employees = conn.Table<Employee>().ToList().Where(c => c.Occupation.Contains(Occupation)).Where(c => c.Department.Contains(DepartmentName)).OrderBy(c => c.LastName).ToList();
            }
            return employees;
        }
    }
}
