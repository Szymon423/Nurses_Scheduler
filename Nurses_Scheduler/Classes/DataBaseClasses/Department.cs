using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.DataBaseClasses
{
    [Table("Department")]
    public class Department
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string DepartmentName { get; set; }

        public string DepartmentShortName { get; set; }

        public override string ToString()
        {
            return $"{DepartmentName} - ({DepartmentShortName})";
        }

        static public List<Department> GetDepartmentsFromDB()
        {
            List<Department> departments = new List<Department>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Employee>();
                departments = conn.Table<Department>().ToList().OrderBy(c => c.DepartmentName).ToList();
            }
            return departments;
        }

    }
}
