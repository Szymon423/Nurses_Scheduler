using Nurses_Scheduler.Classes.RaportClases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Nurses_Scheduler.Classes.Raport
{
    
    public class HTMLpage
    {
        public List<string> Content;
        public List<InternalStyle> InternalStyles;
        
        public HTMLpage()
        {
            Content = new List<string>();
            InternalStyles = new List<InternalStyle>();
        }
        public void AddHeader(int HeaderNum, string headerText)
        {
            string headerToAdd = PutOnBrackets("h" + HeaderNum.ToString(), headerText);
            Content.Add(headerToAdd);
            Debug.WriteLine(headerToAdd);
        }

        public void AddParagraph(string ParagraphText)
        {
            string paragraphToAdd = PutOnBrackets("p", ParagraphText);
            Content.Add(paragraphToAdd);
            Debug.WriteLine(paragraphToAdd);
        }

        public void AddTabele(List<List<string>> TabeleContent)
        {
            string tableString = "";
            for (int i = 0; i < TabeleContent.Count; i++)
            {
                string tableRow = "";
                foreach (string item in TabeleContent[i])
                {
                    string tableData;
                    if (i == 0) // tabele headers
                    {
                        tableData = PutOnBrackets("th", item);
                    }
                    else // tabele data
                    {
                        tableData = PutOnBrackets("td", item);
                    }
                    tableRow += tableData;
                }
                tableString += PutOnBrackets("tr", tableRow);
            }
            Content.Add(PutOnBrackets("table", tableString));
        }

        public void AddStyle(string targetObject, string[] properties)
        {
            InternalStyles.Add(new InternalStyle(targetObject, properties));
        }

        private string PutOnBrackets(string Tag, string content)
        {
            return "<" + Tag + ">" + content + "</" + Tag + ">";
        }

        public void AddStyleToContent()
        {
            string styleString = "";
            foreach (InternalStyle style in InternalStyles)
            {
                styleString += style.getStyle();
            }
            Content.Insert(0, PutOnBrackets("style", styleString));
        }


    }
}
