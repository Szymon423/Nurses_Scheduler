using Nurses_Scheduler.Classes.RaportClases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public void AddPre(string ParagraphText)
        {
            string paragraphToAdd = PutOnBrackets("pre", ParagraphText);
            Content.Add(paragraphToAdd);
            Debug.WriteLine(paragraphToAdd);
        }

        public void AddTabele(List<List<string>> TabeleContent)
        {
            string tableString = "";
            for (int i = 0; i < TabeleContent.Count; i++)
            {
                string tableRow = "";
                for (int j = 0; j < TabeleContent[i].Count; j ++)
                {
                    string item = TabeleContent[i][j];
                    string tableData;
                    if (i == 0) // tabele headers
                    {
                        tableData = "th";
                    }
                    else // tabele data
                    {
                        tableData = "td";
                    }

                    if (j == 1 || j == TabeleContent[i].Count - 1)
                    {
                        string style = "width: 10%;";
                        tableData = PutOnBracketsWithStyle(tableData, item, style);
                    }
                    else
                    {
                        tableData = PutOnBrackets(tableData, item);
                    }
                    tableRow += tableData;
                }
                tableString += PutOnBrackets("tr", tableRow);
            }
            Content.Add(PutOnBrackets("table", tableString));
        }

        public void AddFooterTabele(List<List<string>> TabeleContent)
        {
            string tableString = "";
            for (int i = 0; i < TabeleContent.Count; i++)
            {
                string tableRow = "";
                for (int j = 0; j < TabeleContent[i].Count; j++)
                {
                    string item = TabeleContent[i][j];
                    string tableData = "td";

                    if (i == 1 && j == 3)
                    {
                        string atribute = "rowspan=" + '"' + "6" + '"';
                        tableData = PutOnBracketsWithArtibute(tableData, item, atribute);
                    }
                    else
                    {
                        tableData = PutOnBrackets(tableData, item);
                    }                   
                    tableRow += tableData;
                }
                tableString += PutOnBrackets("tr", tableRow);
            }
            Content.Add(PutOnBrackets("table", tableString));
        }

        public void AddBreakLine()
        {
            Content.Add(PutOnBrackets("br", ""));
        }

        public void AddStyle(string targetObject, string[] properties)
        {
            InternalStyles.Add(new InternalStyle(targetObject, properties));
        }

        private string PutOnBrackets(string Tag, string content)
        {
            return "<" + Tag + ">" + content + "</" + Tag + ">" + "\n";
        }

        private string PutOnBracketsWithStyle(string Tag, string content, string style)
        {              
            return "<" + Tag + " style=" + '"' + style + '"' + ">" + content + "</" + Tag + ">" + "\n";
        }

        private string PutOnBracketsWithArtibute(string Tag, string content, string atribute)
        {
            return "<" + Tag + " " + atribute + ">" + content + "</" + Tag + ">" + "\n";
        }

        public void AddStyleToContent()
        {
            string styleString = "";
            foreach (InternalStyle style in InternalStyles)
            {
                styleString += style.getStyle() + "\n";
            }
            Content.Insert(0, PutOnBrackets("style", styleString));
        }

        public void InsertHTMLtag()
        {
            Content.Insert(0, "<HTML>");
            Content.Add("</HTML>");
        }
    }
}
