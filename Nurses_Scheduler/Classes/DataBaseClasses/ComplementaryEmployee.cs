using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.DataBaseClasses
{
    [Table("ComplementaryEmployee")]
    public class ComplementaryEmployee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int DepartmentId { get; set; }

        public string ShiftType { get; set; }

        public string Occupation { get; set; }

        public static List<ComplementaryEmployee> GetComplementaryEmployeesDB(int _DepartmentId, string EmployeeShiftType)
        {
            List<ComplementaryEmployee> fundamentalEmployees = new List<ComplementaryEmployee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<ComplementaryEmployee>();
                fundamentalEmployees = conn.Table<ComplementaryEmployee>().ToList()
                                                                        .Where(c => c.DepartmentId.Equals(_DepartmentId))
                                                                        .Where(c => c.ShiftType.Equals(EmployeeShiftType))
                                                                        .OrderBy(c => c.Occupation)
                                                                        .ToList();
            }
            return fundamentalEmployees;
        }

        public static List<ComplementaryEmployee> GetComplementaryEmployeesDB(int _DepartmentId)
        {
            List<ComplementaryEmployee> fundamentalEmployees = new List<ComplementaryEmployee>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<ComplementaryEmployee>();
                fundamentalEmployees = conn.Table<ComplementaryEmployee>().ToList()
                                                                        .Where(c => c.DepartmentId.Equals(_DepartmentId))
                                                                        .OrderBy(c => c.Occupation)
                                                                        .ToList();
            }
            return fundamentalEmployees;
        }

    }
}
