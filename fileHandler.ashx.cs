using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Grand_Report
{
    /// <summary>
    /// Summary description for fileHandler
    /// </summary>
    public class fileHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            try
            {
                int ret = 0;
                string str_name = "";
                foreach (string s in context.Request.Files)
                {
                    HttpPostedFile file = context.Request.Files[s];
                    string fileName = file.FileName;
                    string fileExtension = Path.GetExtension(fileName);
                    if (fileExtension.ToLower() == ".csv"|| fileExtension.ToLower() == ".xls" || fileExtension.ToLower() == ".xlsx")
                    {
                        string hexFile = (DateTime.Now).ToString("ss yy mm  dd HH MM");
                        Random random = new Random();
                        str_name = hexFile.Replace(" ", string.Empty) + (random.Next(100, 999)).ToString();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            fileExtension = Path.GetExtension(fileName);
                            str_name = str_name + fileExtension;
                            string pathToSave = HttpContext.Current.Server.MapPath("~/Fileupload/") + str_name;
                            file.SaveAs(pathToSave);
                        }
                        ret = 1;
                    }
                    else
                        ret = 0;
                }
                context.Response.Write(ret == 0 ? "filechk" : str_name);
            }
            catch (Exception ex)
            {
                //context.Response.Write("error");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}