using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MathNet.Numerics.Statistics;

namespace Grand_Report
{
    public partial class Pearson_correlation : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter da;
        SqlDataReader reader;
        // = ConfigurationManager.ConnectionStrings["sqlConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        // Select Table Names
        public List<excelList> cntSql(dbList list)
        {
            List<excelList> xllist = new List<excelList>();
            try
            {
                string connStr = "Data Source=" + list.Host + "; Initial Catalog=" + list.Port + "; User Id=" + list.Userid + ";Password=" + list.Pass;
                DataTable dt = new DataTable();
                con = new SqlConnection();
                con.ConnectionString = connStr;
                con.Open();
                string sqlQury = "SELECT TABLE_NAME FROM " + list.Port + ".INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"; /*, COLUMN_NAME*/
                cmd = new SqlCommand();
                cmd = new SqlCommand(sqlQury, con);
                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                con.Close();
                if (dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                    {
                        excelList excellist = new excelList();
                        excellist.key = "Sheet";
                        excellist.vale = dr["TABLE_NAME"].ToString();
                        excellist.DType = "";// dr["COLUMN_NAME"].ToString();
                        excellist.fileName = null; //list.Table;
                        xllist.Add(excellist);
                    }
                else
                {
                    excelList excellist = new excelList();
                    excellist.key = "Error";
                    excellist.vale = "No tables found";
                    excellist.fileName = list.Table;
                    xllist.Add(excellist);
                }

                return xllist;
            }
            catch (Exception ex)
            {
                excelList excellist = new excelList();
                excellist.key = "Error";
                excellist.vale = ex.ToString();
                excellist.fileName = list.Table;
                xllist.Add(excellist);
                return xllist;
            }

        }

        // Select Column Name
        public List<excelList> cntSqlTable(dbList list)
        {
            List<excelList> xllist = new List<excelList>();
            try
            {
                string connStr = "Data Source=" + list.Host + "; Initial Catalog=" + list.Port + "; User Id=" + list.Userid + ";Password=" + list.Pass;
                DataTable dt = new DataTable();
                con = new SqlConnection();
                con.ConnectionString = connStr;
                con.Open();
                string sqlQury = "SELECT COLUMN_NAME, DATA_TYPE FROM " + list.Port + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + list.Table + "' ORDER BY COLUMN_NAME"; /*, COLUMN_NAME*/
                cmd = new SqlCommand(sqlQury, con);
                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                con.Close();
                if (dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                    {
                        excelList excellist = new excelList();
                        excellist.key = "Column";
                        excellist.vale = dr["COLUMN_NAME"].ToString();
                        excellist.DType = dr["DATA_TYPE"].ToString();
                        excellist.fileName = list.Table;
                        xllist.Add(excellist);
                    }
                else
                {
                    excelList excellist = new excelList();
                    excellist.key = "Error";
                    excellist.vale = "No tables found";
                    excellist.fileName = list.Table;
                    xllist.Add(excellist);
                }

                return xllist;
            }
            catch (Exception ex)
            {
                excelList excellist = new excelList();
                excellist.key = "Error";
                excellist.vale = ex.ToString();
                excellist.fileName = list.Table;
                xllist.Add(excellist);
                return xllist;
            }

        }

        // Create Correlation
        public List<correlationRetList> SqlCorrelation(correlationList list, dbList dblist)
        {
            List<correlationRetList> retlist = new List<correlationRetList>();
            correlationRetList corlist;
            try
            {
                string connStr = "Data Source=" + dblist.Host + "; Initial Catalog=" + dblist.Port + "; User Id=" + dblist.Userid + ";Password=" + dblist.Pass;
                con = new SqlConnection();
                con.ConnectionString = connStr;
                DataTable dt = new DataTable();
                DataTable dtX = new DataTable();
                DataTable dtY = new DataTable();               
                string xVar = "";
                double Target = 0.0;
                var varListTarget = (JArray)JsonConvert.DeserializeObject(list.Target);
                var varListCovariance = (JArray)JsonConvert.DeserializeObject(list.Covariance);
                var varListGrpBy = (JArray)JsonConvert.DeserializeObject(list.GrpBy);
                var varListWhereCon = (JArray)JsonConvert.DeserializeObject(list.WhereCon);

                string whereCon = "", GrpBy = "", sumArg = "";
                foreach (var targetCol in varListTarget)
                {
                    dt = new DataTable();
                    whereCon = ""; GrpBy = ""; sumArg = "";
                    foreach (var Where in varListWhereCon)
                    {
                        if ((String)(JValue)targetCol["tblName"] == (String)(JValue)Where["tblName"])
                            whereCon = whereCon + (whereCon.Length > 0 ? " AND " : "") + (String)(JValue)Where["tblName"] + "." + (String)(JValue)Where["colName"] + " = " + (String)(JValue)Where["wherCond"];
                    }
                    foreach (var Grp in varListGrpBy)
                    {
                        if ((String)(JValue)targetCol["tblName"] == (String)(JValue)Grp["tblName"])
                            GrpBy = GrpBy + (GrpBy.Length > 0 ? " , " : "") + (String)(JValue)Grp["tblName"] + "." + (String)(JValue)Grp["colName"];
                    }
                    whereCon = whereCon.Length > 0 ? " WHERE " + whereCon : "";
                    GrpBy = GrpBy.Length > 0 ? " GROUP BY " + GrpBy : "";
                    sumArg = GrpBy.Length > 0 ? " SUM(" + (String)(JValue)targetCol["colName"] + ") " : "(" + (String)(JValue)targetCol["colName"] + ")";
                    con.Open();
                    string sqlQury = "SELECT " + sumArg + " FROM " + (String)(JValue)targetCol["tblName"] + whereCon + GrpBy + ";";
                    cmd = new SqlCommand();
                    cmd = new SqlCommand(sqlQury, con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                    if (dt.Rows.Count > 0)
                        xVar = xVar + (xVar.Trim().Length > 0 ? ", " : "") + (String)(JValue)targetCol["tblName"] + "." + (String)(JValue)targetCol["colName"];
                        if (dtX.Rows.Count > 0)
                        {
                            int rowCount = dtX.Rows.Count >= dt.Rows.Count ? dtX.Rows.Count : dt.Rows.Count;
                            dt.Columns[0].ReadOnly = false;
                            dt.Columns[0].DataType = typeof(double);
                            for (int i = 0; i < rowCount; i++)
                            {
                                dtX.Rows[i][0] = (i < dtX.Rows.Count ? Convert.ToDouble(dtX.Rows[i][0] == System.DBNull.Value ? "0" : dtX.Rows[i][0].ToString()) : 0) + (i < dt.Rows.Count ? Convert.ToDouble(dt.Rows[i][0] == System.DBNull.Value ? "0" : dt.Rows[i][0].ToString()) : 0);
                            }
                        }
                        else
                        {
                            dtX = dt;
                            dtX.Columns[0].ReadOnly = false;
                            dtX.Columns[0].DataType = typeof(double);
                        }
                }

                foreach (var covarianceCol in varListCovariance)
                {
                    whereCon = ""; GrpBy = ""; sumArg = "";
                    dtY = new DataTable();
                    foreach (var Where in varListWhereCon)
                    {
                        if ((String)(JValue)covarianceCol["tblName"] == (String)(JValue)Where["tblName"])
                            whereCon = whereCon + (whereCon.Length > 0 ? " AND " : "") + (String)(JValue)Where["tblName"] + "." + (String)(JValue)Where["colName"] + " = " + (String)(JValue)Where["wherCond"];
                    }
                    foreach (var Grp in varListGrpBy)
                    {
                        if ((String)(JValue)covarianceCol["tblName"] == (String)(JValue)Grp["tblName"])
                            GrpBy = GrpBy + (GrpBy.Length > 0 ? " , " : "") + (String)(JValue)Grp["tblName"] + "." + (String)(JValue)Grp["colName"];
                    }
                    whereCon = whereCon.Length > 0 ? " WHERE " + whereCon : "";
                    GrpBy = GrpBy.Length > 0 ? " GROUP BY " + GrpBy : "";
                    sumArg = GrpBy.Length > 0 ? " SUM(" + (String)(JValue)covarianceCol["colName"] + ") " : "(" + (String)(JValue)covarianceCol["colName"] + ")";
                    con.Open();
                    string sqlQury = "SELECT " + sumArg + " FROM " + (String)(JValue)covarianceCol["tblName"] + whereCon + GrpBy + ";";
                    cmd = new SqlCommand();
                    cmd = new SqlCommand(sqlQury, con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    dtY.Load(reader);
                    con.Close();
                    Target = Correlations(dtX, dtY);
                    corlist = new correlationRetList();
                    corlist.xVar = xVar;
                    corlist.yVar = ((String)(JValue)covarianceCol["tblName"] + "." + (String)(JValue)covarianceCol["colName"]);
                    corlist.corValue = Target;
                    retlist.Add(corlist);
                }

                return retlist;
            }
            catch (Exception ex)
            {
                retlist = new List<correlationRetList>();
                corlist = new correlationRetList();
                corlist.xVar = "Error";
                corlist.yVar = ex.ToString();
                corlist.corValue = 0.00;
                retlist.Add(corlist);
                return retlist;
            }
        }

        //Calculate Correlation
        public double Correlations(DataTable DouXs, DataTable DouYs)
        {
            return Correlation.Pearson(executeReader(DouXs), executeReader(DouYs));
            //SimpleRegressionModel 
        }

        //Convert DataTable to IEnumerable
        private static IEnumerable<double> executeReader(DataTable reader)
        {
            return reader.AsEnumerable().Select(row => row[0] == System.DBNull.Value ? 0 : Convert.ToDouble(row[0]));
        }


    }
}


//{
//    Convert.ToDouble(row[1])
//            }

//Target = Convert.ToDouble(cmd.ExecuteScalar());

//targetColumn = targetColumn + (targetColumn.Length > 0 ? "+ " : "") + "T" + TableCount + "." + (String)(JValue)targetCol["colName"];
//targetTable = targetTable + (targetTable.Length > 0 ? ", " : "")
//    + "(select AVG(" + (String)(JValue)targetCol["colName"] + ") AS "
//    + (String)(JValue)targetCol["colName"] + " from "
//    + (String)(JValue)targetCol["tblName"] + ") T" + TableCount;
//TableCount++;

//while (reader.Read())
//{
//    yield return (double)reader[0];
//}


//con.Open();
//string sqlQury = "SELECT AVG( " + targetColumn + ") FROM "+ targetTable; 
//cmd = new SqlCommand();
//cmd = new SqlCommand(sqlQury, con);
//Target = Convert.ToDouble(cmd.ExecuteScalar());
//SqlDataReader reader = cmd.ExecuteReader();
//dt.Load(reader);
//con.Close();



//public class DouXs
//{
//    public Double Target { get; set; }
//}

//public double Correlations(DataTable Xs, DataTable Ys)
//{

//    IEnumerable<double> DouXs = Xs.AsEnumerable().Select(row => new
//    {
//        Column1 = Convert.ToDouble(row[1])
//    });




//double sumX = 0;
//double sumX2 = 0;
//double sumY = 0;
//double sumY2 = 0;
//double sumXY = 0;

//int n = Xs.Rows.Count < Ys.Rows.Count ? Xs.Rows.Count : Ys.Rows.Count;

//for (int i = 0; i < n; ++i)
//{
//    double x = Xs.Rows.Count > i ? Convert.ToDouble(Xs.Rows[i][0].ToString().Trim().Length > 0 ? Xs.Rows[i][0].ToString() : "0") : 0;
//    double y = Xs.Rows.Count > i ? Convert.ToDouble(Ys.Rows[i][0].ToString().Trim().Length > 0 ? Ys.Rows[i][0].ToString() : "0") : 0;

//    sumX += x;
//    sumX2 += x * x;
//    sumY += y;
//    sumY2 += y * y;
//    sumXY += x * y;
//}

//double stdX = Math.Sqrt(sumX2 / n - sumX * sumX / n / n);
//double stdY = Math.Sqrt(sumY2 / n - sumY * sumY / n / n);
//double covariance = (sumXY / n - sumX * sumY / n / n);

//return covariance / stdX / stdY;