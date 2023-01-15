using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes
{
    [Table("Employee")]
    internal class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Department { get; set; }

        public string Occupation { get; set; }  

        [Ignore]
        public string FullName 
        { 
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}
