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
        }

        public async Task SaveRaport()
        {
            await File.WriteAllLinesAsync("index.html", html.pages[0].Content);
            string pth = Path.Combine(Directory.GetCurrentDirectory(), "index.html");
            Debug.WriteLine(pth);
            Process.Start(pth);

        }
    }
}
