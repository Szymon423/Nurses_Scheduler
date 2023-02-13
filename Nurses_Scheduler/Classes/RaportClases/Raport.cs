using Nurses_Scheduler.Classes.RaportClases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Printing;
using System.Windows.Documents;


namespace Nurses_Scheduler.Classes.Raport
{
    public class Raport
    {
        private HTML html;
        private List<List<string>> workArrangementList;
        private List<List<List<string>>> workArrangementByOccupationList;
        private List<string> occupationsOnList;
        private RaportData raportData;
        private string raportFileName;
        private static List<List<string>> footer = new List<List<string>>()
        {
            new List<string>()
            {
                "Dzień (D)",
                "7.00 - 19:00",
                "Urlop (U)",
                "Podpis Pielęgniarki Oddziałowej"
            },
            new List<string>()
            {
                "Noc (N)",
                "19:00 - 7:00",
                "Urlop macierzyński (Um)",
                ""
            },
            new List<string>()
            {
                "Rano (r)",
                "",
                "Urlop szkoleniowy (Us)"
            },
            new List<string>()
            {
                "Popołudnie (p)",
                "",
                "Urlop okolicznościowy (Uo)"
            },
            new List<string>()
            {
                "",
                "",
                "Urlop na żądanie (Uż)"
            },
            new List<string>()
            {
                "",
                "",
                "Zwolnienie lekarskie (C)"
            },
            new List<string>()
            {
                "",
                "",
                "Opieka nad dzieckiem (Op)"
            },
        };

        public Raport(RaportData raportData)
        {
            html = new HTML();
            workArrangementList = raportData.departmentWorkArrangement.GetEmployeeWorkArrangementAsList();
            workArrangementByOccupationList = new List<List<List<string>>>();
            occupationsOnList = new List<string>();
            SplitDataIntoSeparateOccupationLists();
            for (int i = 0; i < workArrangementByOccupationList.Count; i++)
            {
                workArrangementByOccupationList[i] = InsertLpAndSignature(workArrangementByOccupationList[i]);
            }
 
            this.raportData = raportData;   
            this.raportFileName = App.months[raportData.month - 1].ToUpper() + " " + raportData.year.ToString() + " " + raportData.department.DepartmentName;
        }

        private void SplitDataIntoSeparateOccupationLists()
        {
            var workArrangementForOneOccupationList = new List<List<string>>();
            foreach ( var row in workArrangementList)
            {
                bool foundHeader = false;
                foreach (string occupation in App.AllowedOccupations)
                {
                    if (row[1].Equals(occupation))
                    {
                        foundHeader = true;
                        break;
                    }
                }
                if (foundHeader)
                {
                    if (workArrangementForOneOccupationList.Count > 0)
                    {
                        workArrangementByOccupationList.Add(workArrangementForOneOccupationList);
                    }
                    occupationsOnList.Add(row[1]);
                    workArrangementForOneOccupationList = new List<List<string>>();
                }
                else
                {
                    workArrangementForOneOccupationList.Add(row);
                }
            }
        }

        public void GenerateRaport()
        {
            for (int i = 0; i < workArrangementByOccupationList.Count; i++)
            {
                var occupationWorkArrangemnent = workArrangementByOccupationList[i];

                HTMLpage page = new HTMLpage();

                page.AddStyle("HTML", new string[]
                {
                "font-family: arial, sans-serif"
                });

                page.AddHeader(2, "Harmonogram pracy (miesiąc, rok) " +
                                    App.months[raportData.month - 1].ToUpper() + " " +
                                    raportData.year.ToString() + " " +
                                    "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +
                                    "Oddział: " + raportData.department.DepartmentName);

                page.AddParagraph("Norma miesięczna godzin: " + raportData.monthlyHours.ToString("N2"));


                page.AddHeader(3, occupationsOnList[i]);
                page.AddStyle("h3", new string[]
                {
                "text-align: center"
                });


                page.AddTabele(occupationWorkArrangemnent, raportData.eventDays, raportData.monthLength);
                page.AddStyle("table", new string[]
                {
                "border-collapse: collapse",
                "width: 100%",
                "table-layout: fixed",
                "background-color: #f7f7f7"
                });

                page.AddStyle("td, th", new string[]
                {
                "border: 1px solid #000000",
                "text-align: left",
                "padding: 8px"
                });

                page.AddStyle("tr:nth-child(even)", new string[]
                {
                "background-color: #ffffff"
                });

                page.AddBreakLine();

                page.AddFooterTabele(footer);



                html.AddPage(page);
            }
        }

        public async Task SaveRaport()
        {
            await File.WriteAllLinesAsync(raportFileName + ".html", html.SavePage(0));
        }

        private static List<List<string>> InsertLpAndSignature(List<List<string>> toChange)
        {
            for (int i = 0; i < toChange.Count; i++)
            {
                if (i == 0)
                {
                    toChange[i].Insert(0, "Lp");
                    toChange[i].Add("Podpis");
                }
                else
                {
                    toChange[i].Insert(0, i.ToString());
                    toChange[i].Add("");
                }
            }
            return toChange;
        }


    }
}
