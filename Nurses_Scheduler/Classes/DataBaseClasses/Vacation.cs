using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.DataBaseClasses
{
    [Table("Vacation")]
    public class Vacation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int startDay { get; set; }

        public int startMonth{ get; set; }

        public int startYear { get; set; }

        public int finishDay { get; set; }
                   
        public int finishMonth { get; set; }
                   
        public int finishYear { get; set; }

        public string vacationType { get; set; }

        [Ignore]
        public string startDateTime
        {
            get
            {
                return startDay.ToString() + "." + startMonth.ToString() + "." + startYear.ToString();
            }
        }

        [Ignore]
        public string finishDateTime
        {
            get
            {
                return finishDay.ToString() + "." + finishMonth.ToString() + "." + finishYear.ToString();
            }
        }

        static public List<Vacation> GetVacationsFromDB()
        {
            List<Vacation> vacations = new List<Vacation>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Vacation>();
                vacations = conn.Table<Vacation>().ToList().OrderBy(c => c.startDateTime).ToList();
            }
            return vacations;
        }

        static public List<Vacation> GetVacationsFromDB(int employeeId)
        {
            List<Vacation> vacations = new List<Vacation>();

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Vacation>();
                vacations = conn.Table<Vacation>().ToList().Where(c => c.EmployeeId.Equals(employeeId)).ToList();
            }
            return vacations;
        }
    }
}
 