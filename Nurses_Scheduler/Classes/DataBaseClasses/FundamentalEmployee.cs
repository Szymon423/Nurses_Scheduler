using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.DataBaseClasses
{

    [Table("FundamentalEmployee")]
    public class FundamentalEmployee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int DepartmentId { get; set; }

        public string ShiftType { get; set; }

        public string Occupation { get; set; }

        public int MinEmployeesNumber { get; set; }

        public static List<FundamentalEmployee> GetFundamentalEmployeesDB(int _DepartmentId, string EmployeeShiftType)
        {
            List<FundamentalEmployee> fundamentalEmployees = new List<FundamentalEmployee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<FundamentalEmployee>();
                fundamentalEmployees = conn.Table<FundamentalEmployee>().ToList()
                                                                        .Where(c => c.DepartmentId.Equals(_DepartmentId))
                                                                        .Where(c => c.ShiftType.Equals(EmployeeShiftType))
                                                                        .OrderBy(c => c.Occupation)
                                                                        .ToList();
            }
            return fundamentalEmployees;
        }

        public static List<FundamentalEmployee> GetFundamentalEmployeesDB(int _DepartmentId)
        {
            List<FundamentalEmployee> fundamentalEmployees = new List<FundamentalEmployee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<FundamentalEmployee>();
                fundamentalEmployees = conn.Table<FundamentalEmployee>().ToList()
                                                                        .Where(c => c.DepartmentId.Equals(_DepartmentId))
                                                                        .OrderBy(c => c.Occupation)
                                                                        .ToList();
            }
            return fundamentalEmployees;
        }

    }
}
