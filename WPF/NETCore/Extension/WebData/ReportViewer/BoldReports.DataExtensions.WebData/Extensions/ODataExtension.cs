using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoldReports.Base.WebDataSource;
using BoldReports.Data;
using BoldReports.RDL.DOM;
using BoldReports.Web;
using BoldReports.Web.DataProviders.Helper;
using BoldReports.WebDatasource.Base.Model;
using BoldReports.Windows;
using BoldReports.Windows.Data;

namespace BoldReports.Data.WebData
{
    //[ExtensionConfig(Name = "OData", Visibility = true)]
    public class ODataExtension : WebAPIExtension
    {
        public ODataExtension()
        {
            this.IsOdata = true;
        }
    }
}
