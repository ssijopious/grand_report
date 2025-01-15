using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grand_Report
{
    public partial class webControlls : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        // DB Connection  

        // Excel
        [System.Web.Services.WebMethod]
        public static List<excelList> webfileDetail(string fileName)
        {
            Index cl = new Index();
            return cl.fileDetail(fileName);
        }

        [System.Web.Services.WebMethod]
        public static List<excelList> webgetHead(excelList list)
        {
            Index cl = new Index();
            return cl.getHead(list);
        }

        // Oracle
        [System.Web.Services.WebMethod]
        public static List<excelList> webcntOracle(dbList list)
        {
            Index cl = new Index();
            return cl.cntOracle(list);
        }

        // SQL Server
        [System.Web.Services.WebMethod] /*Get Table*/
        public static List<excelList> webcntSql(dbList list)
        {
            Pearson_correlation cl = new Pearson_correlation();
            return cl.cntSql(list);
        }

        [System.Web.Services.WebMethod] /*Get Column*/
        public static List<excelList> webcntSqlTable(dbList list)
        {
            Pearson_correlation cl = new Pearson_correlation();
            return cl.cntSqlTable(list);
        }
        //************************************************************************************************************************************//

        // Grand Report Generation

        [System.Web.Services.WebMethod] /*Excel*/
        public static string webgenReport(excelSelectlist list)
        {
            Index cl = new Index();
            if (list.selFlag == "avg")
                return cl.genxlavgReport(list);
            else if (list.selFlag == "dic")
                return cl.genxldicReport(list);
            else
                return "Error : Please select target column";
        }

        [System.Web.Services.WebMethod] /*Oracle*/
        public static string webgenorclReport(dbSelectlist list)
        {
            Index cl = new Index();
            if (list.selFlag == "avg")
                return cl.genorclavgReport(list);
            else if (list.selFlag == "dic")
                return cl.genorcldicReport(list);
            else
                return "Error : Please select target column";
        }
        //************************************************************************************************************************************//

        //Pearson correlation
        [System.Web.Services.WebMethod] /*Sql Correlation*/
        public static List<correlationRetList> webSqlCorrelation(correlationList list, dbList dblist) 
        {
            Pearson_correlation cl = new Pearson_correlation();
            return cl.SqlCorrelation(list, dblist);
        }
        //************************************************************************************************************************************//



        // File Delete

        [System.Web.Services.WebMethod]
        public static string webdeleteFile(string fileName)
        {
            //string[] fileNmae = fileName.Split('/');
            if (File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Fileupload/") + fileName))
            {
                File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Fileupload/") + fileName);
            }
            return null;
        }
        //************************************************************************************************************************************//



    }
}