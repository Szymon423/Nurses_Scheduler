using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Nurses_Scheduler.Classes.Raport
{
    public class Raport
    {
        private HTML html;
        
        public Raport()
        {
            html = new HTML();
            html.AddPage();
            html.pages[0].AddHeader(1, "Hello World");
            
            // testowa lista
            List<List<string>> list = new List<List<string>>();
            for (int i = 0; i < 10; i++)
            {
                List<string> subList = new List<string>();
                for (int j = 0; j < 10; j++)
                {
                    subList.Add(((10 * i) + j).ToString());
                }
                list.Add(subList);
            }
            html.pages[0].AddTabele(list);
            html.pages[0].AddStyle("table", new string[] 
            {
                "font-family: arial, sans-serif",
                "border-collapse: collapse",
                "width: 100 %"
            });
            html.pages[0].AddStyle("td, th", new string[]
            {
                "border: 1px solid #dddddd",
                "text-align: left",
                "padding: 8px"
            });
        }

        public async Task SaveRaport()
        {
            await File.WriteAllLinesAsync("index.html", html.SavePage(0));
            //    I don't know how to open html in browser ???
            //    string pth = Path.Combine(Directory.GetCurrentDirectory(), "index.html");
            //    Debug.WriteLine(pth);
            //    Process.Start(pth);
        }
    }
}
