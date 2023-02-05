using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Nurses_Scheduler.Classes.Raport
{
    public class Raport
    {
        private HTML html;
        
        public Raport(List<List<string>> workArrangementList)
        {
            html = new HTML();
            html.AddPage();
            html.pages[0].AddHeader(1, "Harmonogram pracy (miesiąc, rok) ...");
            

            html.pages[0].AddTabele(workArrangementList);

            html.pages[0].AddStyle("table", new string[] 
            {
                "font-family: arial, sans-serif",
                "border-collapse: collapse",
                "width: 100%"
            });

            html.pages[0].AddStyle("td, th", new string[]
            {
                "border: 1px solid #000000",
                "text-align: left",
                "padding: 8px"
            });

            html.pages[0].AddStyle("tr:nth-child(even)", new string[]
            {
                "background-color: #dddddd"
            });
        }

        public async Task SaveRaport()
        {
            await File.WriteAllLinesAsync("index.html", html.SavePage(0));
            string messageBoxText = "Raport został wygenerowany.";
            string caption = "Raport";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
        }
    }
}
