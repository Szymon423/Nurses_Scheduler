using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.Raport
{
    public class HTML
    {
        public List<HTMLpage> pages;

        public HTML()
        {
            pages = new List<HTMLpage>();   
        }

        public void AddPage(HTMLpage page)
        {
            pages.Add(page);
        }

        public List<string> SavePage(int pageNumber)
        {
            pages[pageNumber].InsertHTMLtag();
            pages[pageNumber].AddStyleToContent();    
            return pages[pageNumber].Content;
        }     
    }
}
