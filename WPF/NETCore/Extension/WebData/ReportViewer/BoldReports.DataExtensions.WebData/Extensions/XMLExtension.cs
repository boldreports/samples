using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoldReports.Data;
using BoldReports.RDL.DOM;
using BoldReports.Web;

namespace BoldReports.Data.WebData
{
    //[ExtensionConfig(Name = "XML", Visibility = true)]
    public class XMLExtension : JSONExtension
    {
        public XMLExtension()
        {
            this.IsXML = true;
        }
    }
}
