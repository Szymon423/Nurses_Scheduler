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

        public string Department { get; set; }

        public string ShiftType { get; set; }

        public string Occupation { get; set; }

        public int MinEmployeesNumber { get; set; }
    }
}
