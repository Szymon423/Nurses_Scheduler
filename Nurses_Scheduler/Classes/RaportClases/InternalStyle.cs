using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nurses_Scheduler.Classes.RaportClases
{
    public class InternalStyle
    {
        private string targetObject;
        private string[] properties;
        
        public InternalStyle(string targetObject, string[] properties)
        {
            this.targetObject = targetObject;
            this.properties = properties;
        }

        public string getStyle()
        {
            string toReturn = targetObject + "{";
            foreach (string property in properties)
            {
                toReturn += property + ";";
            }
            toReturn += "}";
            return toReturn;
        }
    }
}
