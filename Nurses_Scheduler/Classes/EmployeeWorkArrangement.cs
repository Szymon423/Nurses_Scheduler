using Nurses_Scheduler.Classes.DataBaseClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes
{
    public class EmployeeWorkArrangement
    {
        public EmployeeWorkArrangement(Employee employee)
        {
            this.employee = employee;
            this._Pracownik = employee.FullName;
            this._1 = "";
            this._2 = "";
            this._3 = "";
            this._4 = "";
            this._5 = "";
            this._6 = "";
            this._7 = "";
            this._8 = "";
            this._9 = "";
            this._10 = "";
            this._11 = "";
            this._12 = "";
            this._13 = "";
            this._14 = "";
            this._15 = "";
            this._16 = "";
            this._17 = "";
            this._18 = "";
            this._19 = "";
            this._20 = "";
            this._21 = "";
            this._22 = "";
            this._23 = "";
            this._24 = "";
            this._25 = "";
            this._26 = "";
            this._27 = "";
            this._28 = "";
            this._29 = "";
            this._30 = "";
            this._31 = "";
        }

        public EmployeeWorkArrangement(string employeeGroup)
        {
            this._Pracownik = employeeGroup;
            this._1 = "";
            this._2 = "";
            this._3 = "";
            this._4 = "";
            this._5 = "";
            this._6 = "";
            this._7 = "";
            this._8 = "";
            this._9 = "";
            this._10 = "";
            this._11 = "";
            this._12 = "";
            this._13 = "";
            this._14 = "";
            this._15 = "";
            this._16 = "";
            this._17 = "";
            this._18 = "";
            this._19 = "";
            this._20 = "";
            this._21 = "";
            this._22 = "";
            this._23 = "";
            this._24 = "";
            this._25 = "";
            this._26 = "";
            this._27 = "";
            this._28 = "";
            this._29 = "";
            this._30 = "";
            this._31 = "";
        }

        public Employee employee { get; set; }
        public string _Pracownik { get; set; }
        public string _1 { get; set; }
        public string _2 { get; set; }
        public string _3 { get; set; }
        public string _4 { get; set; }
        public string _5 { get; set; }
        public string _6 { get; set; }
        public string _7 { get; set; }
        public string _8 { get; set; }
        public string _9 { get; set; }
        public string _10 { get; set; }
        public string _11 { get; set; }
        public string _12 { get; set; }
        public string _13 { get; set; }
        public string _14 { get; set; }
        public string _15 { get; set; }
        public string _16 { get; set; }
        public string _17 { get; set; }
        public string _18 { get; set; }
        public string _19 { get; set; }
        public string _20 { get; set; }
        public string _21 { get; set; }
        public string _22 { get; set; }
        public string _23 { get; set; }
        public string _24 { get; set; }
        public string _25 { get; set; }
        public string _26 { get; set; }
        public string _27 { get; set; }
        public string _28 { get; set; }
        public string _29 { get; set; }
        public string _30 { get; set; }
        public string _31 { get; set; }

        public List<string> GetWorkArrangementAsListWithEmployeeData()
        {
            List<string> workArrangementAsList = new List<string>();
            if (employee == null)
            {
                // workArrangementAsList.Add(_Pracownik);
                return workArrangementAsList;
            }
            else
            {
                workArrangementAsList.Add(employee.Id.ToString());
            }
            workArrangementAsList.Add(_1);
            workArrangementAsList.Add(_2);
            workArrangementAsList.Add(_3);
            workArrangementAsList.Add(_4);
            workArrangementAsList.Add(_5);
            workArrangementAsList.Add(_6);
            workArrangementAsList.Add(_7);
            workArrangementAsList.Add(_8);
            workArrangementAsList.Add(_9);
            workArrangementAsList.Add(_10);
            workArrangementAsList.Add(_11);
            workArrangementAsList.Add(_12);
            workArrangementAsList.Add(_13);
            workArrangementAsList.Add(_14);
            workArrangementAsList.Add(_15);
            workArrangementAsList.Add(_16);
            workArrangementAsList.Add(_17);
            workArrangementAsList.Add(_18);
            workArrangementAsList.Add(_19);
            workArrangementAsList.Add(_20);
            workArrangementAsList.Add(_21);
            workArrangementAsList.Add(_22);
            workArrangementAsList.Add(_23);
            workArrangementAsList.Add(_24);
            workArrangementAsList.Add(_25);
            workArrangementAsList.Add(_26);
            workArrangementAsList.Add(_27);
            workArrangementAsList.Add(_28);
            workArrangementAsList.Add(_29);
            workArrangementAsList.Add(_30);
            workArrangementAsList.Add(_31);

            return workArrangementAsList;
        }

        public List<string> GetWorkArrangementAsList()
        {
            List<string> workArrangementAsList = new List<string>();
            workArrangementAsList.Add(_Pracownik);
            workArrangementAsList.Add(_1);
            workArrangementAsList.Add(_2);
            workArrangementAsList.Add(_3);
            workArrangementAsList.Add(_4);
            workArrangementAsList.Add(_5);
            workArrangementAsList.Add(_6);
            workArrangementAsList.Add(_7);
            workArrangementAsList.Add(_8);
            workArrangementAsList.Add(_9);
            workArrangementAsList.Add(_10);
            workArrangementAsList.Add(_11);
            workArrangementAsList.Add(_12);
            workArrangementAsList.Add(_13);
            workArrangementAsList.Add(_14);
            workArrangementAsList.Add(_15);
            workArrangementAsList.Add(_16);
            workArrangementAsList.Add(_17);
            workArrangementAsList.Add(_18);
            workArrangementAsList.Add(_19);
            workArrangementAsList.Add(_20);
            workArrangementAsList.Add(_21);
            workArrangementAsList.Add(_22);
            workArrangementAsList.Add(_23);
            workArrangementAsList.Add(_24);
            workArrangementAsList.Add(_25);
            workArrangementAsList.Add(_26);
            workArrangementAsList.Add(_27);
            workArrangementAsList.Add(_28);
            workArrangementAsList.Add(_29);
            workArrangementAsList.Add(_30);
            workArrangementAsList.Add(_31);

            return workArrangementAsList;
        }


        public string GetSingleWorkArrangement(int dayInMonth)
        {
            switch (dayInMonth)
            {
                case 1: { return _1; }
                case 2: { return _2; }
                case 3: { return _3; }
                case 4: { return _4; }
                case 5: { return _5; }
                case 6: { return _6; }
                case 7: { return _7; }
                case 8: { return _8; }
                case 9: { return _9; }
                case 10: { return _10; }
                case 11: { return _11; }
                case 12: { return _12; }
                case 13: { return _13; }
                case 14: { return _14; }
                case 15: { return _15; }
                case 16: { return _16; }
                case 17: { return _17; }
                case 18: { return _18; }
                case 19: { return _19; }
                case 20: { return _20; }
                case 21: { return _21; }
                case 22: { return _22; }
                case 23: { return _23; }
                case 24: { return _24; }
                case 25: { return _25; }
                case 26: { return _26; }
                case 27: { return _27; }
                case 28: { return _28; }
                case 29: { return _29; }
                case 30: { return _30; }
                case 31: { return _31; }
                default: { Debug.WriteLine("Wrong day: " + dayInMonth.ToString()); break; }
            }
            return "";
        }

        public void SetEmployeeWorkArrangement(int dayInMonth, string value)
        {
            switch (dayInMonth)
            {
                case 1: { _1 = value; break; }
                case 2: { _2 = value; break; }
                case 3: { _3 = value; break; }
                case 4: { _4 = value; break; }
                case 5: { _5 = value; break; }
                case 6: { _6 = value; break; }
                case 7: { _7 = value; break; }
                case 8: { _8 = value; break; }
                case 9: { _9 = value; break; }
                case 10: { _10 = value; break; }
                case 11: { _11 = value; break; }
                case 12: { _12 = value; break; }
                case 13: { _13 = value; break; }
                case 14: { _14 = value; break; }
                case 15: { _15 = value; break; }
                case 16: { _16 = value; break; }
                case 17: { _17 = value; break; }
                case 18: { _18 = value; break; }
                case 19: { _19 = value; break; }
                case 20: { _20 = value; break; }
                case 21: { _21 = value; break; }
                case 22: { _22 = value; break; }
                case 23: { _23 = value; break; }
                case 24: { _24 = value; break; }
                case 25: { _25 = value; break; }
                case 26: { _26 = value; break; }
                case 27: { _27 = value; break; }
                case 28: { _28 = value; break; }
                case 29: { _29 = value; break; }
                case 30: { _30 = value; break; }
                case 31: { _31 = value; break; }
                default: { Debug.WriteLine("Wrong day: " + dayInMonth.ToString()); break; }
            }
        }
    }
}
    