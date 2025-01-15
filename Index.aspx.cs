using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Text;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System.Numerics;
//using Python.Runtime;
//using System.Text.Json;
//using Newtonsoft;


namespace Grand_Report
{
    public class Ilist
    {
        public string vale { get; set; }
    }

    public class excelList
    {
        public string key { get; set; }
        public string vale { get; set; }
        public string fileName { get; set; }
        public string DType { get; set; }
    }

    public class excelSelectlist
    {
        public string sheet { get; set; }
        public string selCol { get; set; }
        public string selAvgCol { get; set; }
        public string fileName { get; set; }
        public string selFlag { get; set; }
    }

    public class dbList
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Sid { get; set; }
        public string Table { get; set; }
        public string Userid { get; set; }
        public string Pass { get; set; }
    }

    public class correlationDataList
    {
        public string tblName { get; set; }
        public string colName { get; set; }
        public string wherCond { get; set; }
    }

    public class correlationList
    {
        public string Target { get; set; }
        public string Covariance { get; set; }
        public string WhereCon { get; set; }
        public string GrpBy { get; set; }
    }

    public class correlationRetList
    {
        public string xVar { get; set; }
        public string yVar { get; set; }
        public Double corValue { get; set; }
    }

    public class dbSelectlist
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Sid { get; set; }
        public string Table { get; set; }
        public string Userid { get; set; }
        public string Pass { get; set; }
        public string selAvgCol { get; set; }
        public string selCol { get; set; }
        public string selFlag { get; set; }
    }

    public partial class Index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public List<excelList> fileDetail(string fileName)
        {
            List<excelList> list = new List<excelList>();
            string[] file = fileName.Split('.');
            if (file[1].ToLower() == "csv")
                list = opencsv(fileName);
            else
                list = openxl(fileName);
            return list;
        }

        public List<excelList> getHead(excelList list)
        {
            return selectxlhead(list.fileName, list.vale);
        }

        public List<excelList> cntOracle(dbList list)
        {
            return cntOracletable(list);
        }

        public string genxlavgReport(excelSelectlist list)
        {
            string conString = getxlConstr(list.fileName);
            OleDbConnection connExcel = new OleDbConnection(conString);
            try
            {
                OleDbCommand cmdExcel = new OleDbCommand();
                OleDbDataAdapter oda = new OleDbDataAdapter();
                DataTable dt = new DataTable();
                DataTable dt_out = new DataTable();
                DataTable dt_corr = new DataTable();
                DataTable dtExcelSchema;
                cmdExcel.Connection = connExcel;
                string sheetName = "";
                double Alift = 0.0, totalCount = 0.0;
                if (list.sheet.Trim().Length < 1 || list.sheet == "Select")
                {
                    connExcel.Open();
                    dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    sheetName = dtExcelSchema.Rows.Count > 1 ? list.sheet : dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                    connExcel.Close();
                }
                else
                    sheetName = list.sheet;
                string[] selAvgCol = null;
                string avgQry = "", liftavgQry = "", confiavgQry = "";
                //selAvgCol = list.selAvgCol.Split(',');
                if (list.selAvgCol.Contains(','))
                {
                    selAvgCol = list.selAvgCol.Split(',');
                    foreach (var avgCol in selAvgCol)
                        avgQry = avgQry + (avgQry.Trim().Length > 0 ? ", " : "") + "format((Avg (" + avgCol + ")), '0.00') AS AVG_" + avgCol;
                }

                else
                    avgQry = "format((Avg (" + list.selAvgCol + ")), '0.00') AS AVG_" + list.selAvgCol;

                connExcel.Open();
                cmdExcel.CommandText = " SELECT count(*) AS Support, " +
                                       avgQry + //" format((Avg (" + list.selAvgCol + ")), '0.00') AS AVG_" + list.selAvgCol +
                                       " From [" + sheetName + "]";
                oda.SelectCommand = cmdExcel;
                oda.Fill(dt);
                connExcel.Close();

                if (list.selAvgCol.Contains(','))
                {
                    Alift = ((Convert.ToDouble(dt.Rows[0]["AVG_" + selAvgCol[0]].ToString())) + (Convert.ToDouble(dt.Rows[0]["AVG_" + selAvgCol[1]].ToString()))) / 2;
                    liftavgQry = "((Avg(" + selAvgCol[0] + ")) + (Avg(" + selAvgCol[1] + "))) / 2";
                    confiavgQry = "(sum(" + selAvgCol[0] + " + " + selAvgCol[1] + "))";
                }
                else
                {
                    Alift = Convert.ToDouble(dt.Rows[0]["AVG_" + list.selAvgCol].ToString());
                    liftavgQry = "Avg (" + list.selAvgCol + ")";
                    confiavgQry = "sum(" + list.selAvgCol + ")";
                }
                totalCount = Convert.ToDouble(dt.Rows[0]["Support"].ToString());

                if (list.selCol == "All")
                {
                    List<excelList> exlList = selectxlhead(list.fileName, sheetName);
                    dt = new DataTable();
                    List<excelList> cobolist = new List<excelList>();
                    foreach (var listhead in exlList)
                    {
                        if (list.selAvgCol.Contains(','))
                        {
                            if (listhead.vale != selAvgCol[0] && listhead.vale != selAvgCol[1])
                                cobolist.Add(listhead);
                        }

                        else
                        {
                            if (listhead.vale != list.selAvgCol)
                                cobolist.Add(listhead);//exlList.Remove(listhead);
                        }
                    }
                    allcombo allcombo = new allcombo();
                    List<Tuple<string, int>> colCombo = allcombo.GetCombination(cobolist);
                    foreach (var colcombo in colCombo)
                    {
                        dt = new DataTable();
                        connExcel.Open();
                        double correlation;
                        cmdExcel.CommandText = " SELECT format(Count(*)/" + totalCount + ",'0.000') AS SUPPORT, " +
                                              " format((" + Alift + "/(" + liftavgQry + ")), '0.00') AS LIFT, " +
                                              //"format(( (" + confiavgQry + ") /(Count(*))), '0.000') AS CONFIDENCE, " +
                                              /*(list.selAvgCol.Contains(',') ? "format((" + liftavgQry + "), '0.00') AS AVG_Target, " : "") +*/ avgQry + " ,  " + colcombo.Item1 +
                                              " From [" + sheetName + "] GROUP BY " + colcombo.Item1;

                        oda.SelectCommand = cmdExcel;
                        oda.Fill(dt);
                        connExcel.Close();

                        if (list.selAvgCol.Contains(','))
                        {
                            dt_corr = new DataTable();
                            foreach (DataRow dr in dt.Rows)
                            {

                                double rSquared, intercept, slope;
                                string wherCon = "";
                                string[] wherestring = colcombo.Item1.Split(',');
                                foreach (var colmname in wherestring)// int i = 6; i < dt.Columns.Count; i++)
                                {
                                    wherCon = wherCon + (wherCon.Trim().Length > 0 ? " And " : "") + colmname + " = '" + dr[colmname.TrimStart()] + "'";

                                }
                                connExcel.Open();
                                cmdExcel.CommandText = " SELECT " + selAvgCol[0] + ", " + selAvgCol[1] + " From [" + sheetName + "] WHERE  " + wherCon;
                                oda.SelectCommand = cmdExcel;
                                oda.Fill(dt_corr);
                                connExcel.Close();
                                if (dt_corr.Rows.Count > 0)
                                {
                                    correlation = 0;
                                    if (dt.Columns.Contains("Correltion"))
                                    {
                                        correlation = Correlation.Pearson(executeReader(dt_corr, 0), executeReader(dt_corr, 1));
                                        dr["Correltion"] = correlation.ToString("0.##");
                                        LinearRegression(dt_corr.AsEnumerable().Select(r => r.Field<Double>(0)).ToArray(), dt_corr.AsEnumerable().Select(r => r.Field<Double>(1)).ToArray(), out rSquared, out intercept, out slope);
                                        dr["Coefficient (" + selAvgCol[0] + ")"] = intercept.ToString("0.##");
                                        dr["Coefficient (" + selAvgCol[1] + ")"] = slope.ToString("0.##");

                                        dr["t Stat"] = ((correlation * (Math.Sqrt(dt_corr.Rows.Count - 2))) / (1 - (correlation * correlation))).ToString("0.##");
                                        
                                    }
                                    else
                                    {
                                        System.Data.DataColumn newColumn = new System.Data.DataColumn("Correltion", typeof(System.Double));
                                        correlation = Correlation.Pearson(executeReader(dt_corr, 0), executeReader(dt_corr, 1));
                                        newColumn.DefaultValue = correlation.ToString("0.##");
                                        dt.Columns.Add(newColumn);
                                        System.Data.DataColumn newColumncoef1 = new System.Data.DataColumn("Coefficient (" + selAvgCol[0] + ")", typeof(System.Double));
                                        System.Data.DataColumn newColumncoef2 = new System.Data.DataColumn("Coefficient (" + selAvgCol[1] + ")", typeof(System.Double));
                                        LinearRegression(dt_corr.AsEnumerable().Select(r => r.Field<Double>(0)).ToArray(), dt_corr.AsEnumerable().Select(r => r.Field<Double>(1)).ToArray(), out rSquared, out intercept, out slope);
                                        newColumncoef1.DefaultValue = intercept.ToString("0.##");
                                        newColumncoef2.DefaultValue = slope.ToString("0.##");
                                        dt.Columns.Add(newColumncoef1);
                                        dt.Columns.Add(newColumncoef2);

                                        System.Data.DataColumn newColumncoef3 = new System.Data.DataColumn("t Stat", typeof(System.Double));
                                        newColumncoef3.DefaultValue = ((correlation * (Math.Sqrt(dt_corr.Rows.Count - 2))) / (1 - (correlation * correlation))).ToString("0.##");
                                        dt.Columns.Add(newColumncoef3);
                                    }
                                }
                            }
                            dt_out.Merge(dt);
                            dt_out.Columns["Correltion"].SetOrdinal(4);//6
                            dt_out.Columns["Coefficient (" + selAvgCol[0] + ")"].SetOrdinal(5);
                            dt_out.Columns["Coefficient (" + selAvgCol[1] + ")"].SetOrdinal(6);
                            dt_out.Columns["t Stat"].SetOrdinal(7);
                        }
                        else
                            dt_out.Merge(dt);


                    }
                    connExcel.Dispose();
                }
                else if (list.selCol.Trim().Length > 0)
                {
                    dt = new DataTable();
                    dt_corr = new DataTable();
                    dt_out = new DataTable();
                    double rSquared, intercept, slope, correlation;
                    connExcel.Open();
                    cmdExcel.CommandText = " SELECT format(Count(*)/" + totalCount + ",'0.000') AS SUPPORT, " +
                                           " format((" + Alift + "/(" + liftavgQry + ")), '0.000') AS LIFT, " +
                                           //"format(( (" + confiavgQry + ") /(Count(*))), '0.000') AS CONFIDENCE, " +
                                          /* (list.selAvgCol.Contains(',') ? "format((" + liftavgQry + "), '0.00') AS AVG_Target, " : "") +*/ avgQry + ", " + list.selCol +//" format((Avg (" + list.selAvgCol + ")), '0.000') AS AVG_" + list.selAvgCol
                                           " From [" + sheetName + "] GROUP BY " + list.selCol;
                    oda.SelectCommand = cmdExcel;
                    oda.Fill(dt);
                    cmdExcel.CommandText = " SELECT " + list.selAvgCol + ", "  + list.selCol + " From [" + sheetName + "] "; //+"WHERE  " + wherCon;
                    oda.SelectCommand = cmdExcel;
                    oda.Fill(dt_corr);
                    connExcel.Close();
                    //MultipleLinearRegression
                    //PythonEngine.Initialize();
                    //dynamic multiplyNumbersModule = PythonEngine.ModuleFromString("multiply_numbers", File.ReadAllText("multiply_numbers.py"));

                    if (list.selAvgCol.Contains(','))
                    {

                        
                        foreach (DataRow dr in dt.Rows)
                        {
                            dt_corr = new DataTable();
                            string wherCon = "";
                            string[] wherestring = list.selCol.Split(',');
                            foreach (var colmname in wherestring)// int i = 6; i < dt.Columns.Count; i++)
                            {
                                wherCon = wherCon + (wherCon.Trim().Length > 0 ? " And " : "") + colmname + " = '" + dr[colmname.TrimStart()] + "'";

                            }
                            connExcel.Open();
                            cmdExcel.CommandText = " SELECT " + selAvgCol[0] + ", " + selAvgCol[1] + " From [" + sheetName + "] WHERE  " + wherCon;
                            oda.SelectCommand = cmdExcel;
                            oda.Fill(dt_corr);
                            connExcel.Close();
                            if (dt_corr.Rows.Count > 0)
                            {
                                correlation = 0;
                                if (dt.Columns.Contains("Correltion"))
                                {
                                    correlation = Correlation.Pearson(executeReader(dt_corr, 0), executeReader(dt_corr, 1));
                                    dr["Correltion"] = correlation.ToString("0.##");
                                    LinearRegression(dt_corr.AsEnumerable().Select(r => r[0] == DBNull.Value ? 0.00 : r.Field<Double>(0)).ToArray(), dt_corr.AsEnumerable().Select(r => r[1] == DBNull.Value ? 0.00 :  r.Field<Double>(1)).ToArray(), out rSquared, out intercept, out slope);
                                    dr["Coefficient (" + selAvgCol[0] + ")"] = intercept.ToString("0.##");
                                    dr["Coefficient (" + selAvgCol[1] + ")"] = slope.ToString("0.##");

                                    dr["t Stat"] = ((correlation * (Math.Sqrt(dt_corr.Rows.Count - 2))) / (1 - (correlation * correlation))).ToString("0.##");
                                }
                                else
                                {
                                    System.Data.DataColumn newColumn = new System.Data.DataColumn("Correltion", typeof(System.Double));
                                    correlation = Correlation.Pearson(executeReader(dt_corr, 0), executeReader(dt_corr, 1));
                                    newColumn.DefaultValue = correlation.ToString("0.##");
                                    dt.Columns.Add(newColumn);
                                    System.Data.DataColumn newColumncoef1 = new System.Data.DataColumn("Coefficient (" + selAvgCol[0] + ")", typeof(System.Double));
                                    System.Data.DataColumn newColumncoef2 = new System.Data.DataColumn("Coefficient (" + selAvgCol[1] + ")", typeof(System.Double));
                                    LinearRegression(dt_corr.AsEnumerable().Select(r => r[0] == DBNull.Value? 0.00: r.Field<Double>(0)).ToArray(), dt_corr.AsEnumerable().Select(r => r[1] == DBNull.Value ? 0.00 : r.Field<Double>(1)).ToArray(), out rSquared, out intercept, out slope);
                                    newColumncoef1.DefaultValue = intercept.ToString("0.##");
                                    newColumncoef2.DefaultValue = slope.ToString("0.##");
                                    dt.Columns.Add(newColumncoef1);
                                    dt.Columns.Add(newColumncoef2);

                                    System.Data.DataColumn newColumncoef3 = new System.Data.DataColumn("t Stat", typeof(System.Double));
                                    newColumncoef3.DefaultValue = ((correlation * (Math.Sqrt(dt_corr.Rows.Count - 2))) / (1 - (correlation * correlation))).ToString("0.##");
                                    dt.Columns.Add(newColumncoef3);
                                }
                            }
                        }
                        dt_out.Merge(dt);
                        dt_out.Columns["Correltion"].SetOrdinal(3 + list.selAvgCol.Count(f => (f == ',')));//6
                        dt_out.Columns["Coefficient (" + selAvgCol[0] + ")"].SetOrdinal(5);
                        dt_out.Columns["Coefficient (" + selAvgCol[1] + ")"].SetOrdinal(6);
                        dt_out.Columns["t Stat"].SetOrdinal(7);
                    }
                    else
                        dt_out = dt;

                    connExcel.Dispose();
                }
                else
                {
                    connExcel.Dispose();
                }

                return DTtoJSON(dt_out.DefaultView.ToTable(true));
            }
            catch (Exception ex)
            {
                connExcel.Close();
                return "Error : " + ex.Message;
            }
        }

        public string genxldicReport(excelSelectlist list)
        {
            string conString = getxlConstr(list.fileName);
            OleDbConnection connExcel = new OleDbConnection(conString);
            try
            {
                OleDbCommand cmdExcel = new OleDbCommand();
                OleDbDataAdapter oda = new OleDbDataAdapter();
                DataTable dt = new DataTable();
                DataTable dtExcelSchema;
                cmdExcel.Connection = connExcel;
                string sheetName = "";
                double totalCount = 0.0;
                if (list.sheet.Trim().Length < 1)
                {
                    connExcel.Open();
                    dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    sheetName = dtExcelSchema.Rows.Count > 1 ? list.sheet : dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                    connExcel.Close();
                }
                else
                    sheetName = list.sheet;

                dt = new DataTable();
                connExcel.Open();
                cmdExcel.CommandText = " Select Count(*) AS Support From [" + sheetName + "] ";
                oda.SelectCommand = cmdExcel;
                oda.Fill(dt);
                connExcel.Close();
                if (dt.Rows.Count > 0)
                {
                    totalCount = Convert.ToDouble(dt.Rows[0]["Support"].ToString());
                    string[] selCol = list.selCol.Split(',');
                    string cselCol = "";
                    if (selCol.Count() > 1)
                        foreach (string st in selCol)
                        {
                            cselCol = cselCol + (cselCol == "" ? "where C." + st + "= A." + st : " AND C." + st + "= A." + st);
                        }
                    dt = new DataTable();
                    connExcel.Open();
                    cmdExcel.CommandText = " Select ROUND(Count(*)/" + totalCount + ",3) AS SUPPORT," +
                                           " ROUND(((count(*)/" + totalCount + ") / ((select  Count(*) From [" + sheetName + "] B where B." + list.selAvgCol + " = A." + list.selAvgCol + ")/" + totalCount + "))" +
                                           " /((select  Count(*) From [" + sheetName + "] C " + cselCol + ")/" + totalCount + "),3)  AS LIFT, " +
                                           " ROUND((count(*)/" + totalCount + ") / ((select  Count(*) From [" + sheetName + "] D where D." + list.selAvgCol + " = A." + list.selAvgCol + ")/" + totalCount + "),3)  AS CONDITIONAL_PROBABILITY, " +
                                           "" + list.selAvgCol + " , " + list.selCol + "  From [" + sheetName + "] A group by " + list.selAvgCol + ", " + list.selCol + "" +
                                           " order by " + list.selAvgCol + ", " + list.selCol + "";
                    oda.SelectCommand = cmdExcel;
                    oda.Fill(dt);
                    connExcel.Close();
                    connExcel.Dispose();
                    return DTtoJSON(dt);
                }
                else
                {
                    connExcel.Close();
                    connExcel.Dispose();
                    return "Error : No record found";
                }
            }
            catch (Exception ex)
            {
                connExcel.Close();
                return "Error : " + ex.Message;
            }
        }

        public string genorclavgReport(dbSelectlist list)
        {
            DataTable dt = new DataTable();
            OracleConnection con = new OracleConnection();
            string orclQuery = "";
            double Alift = 0.0, totalCount = 0.0;
            string connStr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=" + list.Host + " )(PORT=" + list.Port + "))(CONNECT_DATA=(SID=" + list.Sid + ")));User Id=" + list.Userid + ";Password=" + list.Pass + "";
            try
            {
                orclQuery = " Select count(*) AS Support, " +
                            " ROUND(Avg(" + list.selAvgCol + "),2) AS AVG_" + list.selAvgCol +
                            " from " + list.Table + "";
                con.ConnectionString = connStr;
                using (OracleCommand cmd = new OracleCommand(orclQuery, con))
                {
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dt);
                    con.Close();
                }

                if (list.selCol == "All")
                {
                    dbList dblist = new dbList();
                    dblist.Host = list.Host;
                    dblist.Port = list.Port;
                    dblist.Sid = list.Sid;
                    dblist.Table = list.Table;
                    dblist.Userid = list.Userid;
                    dblist.Pass = list.Pass;
                    Alift = Convert.ToDouble(dt.Rows[0]["AVG_" + list.selAvgCol].ToString());
                    totalCount = Convert.ToDouble(dt.Rows[0]["Support"].ToString());
                    dt = new DataTable();
                    List<excelList> exlList = cntOracletable(dblist);
                    for (int s = 0; s < exlList.Count; s++)
                    {
                        if (exlList[s].vale == list.selAvgCol)
                            exlList.RemoveAt(s);
                    }

                    allcombo allcombo = new allcombo();
                    List<Tuple<string, int>> colCombo = allcombo.GetCombination(exlList);
                    foreach (var colcombo in colCombo)
                    {
                        orclQuery = " Select  ROUND(Count(*)/" + totalCount + ",3) AS SUPPORT," +
                                                                            " ROUND((" + Alift + "/Avg(" + list.selAvgCol + ")),2) AS LIFT, " +
                                                                            " ROUND(Avg(" + list.selAvgCol + "),2) AS AVG_" + list.selAvgCol + ",  " + colcombo.Item1 +
                                                                            " from " + list.Table + "  group by " + colcombo.Item1 + " order by " + colcombo.Item1 + "";
                        con.ConnectionString = connStr;
                        using (OracleCommand cmd = new OracleCommand(orclQuery, con))
                        {
                            cmd.CommandType = CommandType.Text;
                            con.Open();
                            OracleDataAdapter da = new OracleDataAdapter(cmd);
                            da.Fill(dt);
                            con.Close();
                        }
                    }
                    con.Dispose();
                }
                else if (list.selCol.Trim().Length > 0)
                {
                    Alift = Convert.ToDouble(dt.Rows[0]["AVG_" + list.selAvgCol].ToString());
                    totalCount = Convert.ToDouble(dt.Rows[0]["Support"].ToString());
                    dt = new DataTable();
                    orclQuery = " Select  ROUND(Count(*)/" + totalCount + ",3) AS SUPPORT," +
                                " ROUND((" + Alift + "/Avg(" + list.selAvgCol + ")),2) AS LIFT, " +
                                " ROUND(Avg(" + list.selAvgCol + "),2) AS AVG_" + list.selAvgCol + ",  " + list.selCol +
                                " from " + list.Table + "  group by " + list.selCol + " order by " + list.selCol + "";
                    con.ConnectionString = connStr;
                    using (OracleCommand cmd = new OracleCommand(orclQuery, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        con.Open();
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);
                        con.Close();
                        con.Dispose();
                    }
                }
                else
                {
                    con.Dispose();
                }
                return DTtoJSON(dt);
            }
            catch (Exception ex)
            {
                con.Close();
                con.Dispose();
                return "Error : " + ex.Message;
            }
        }

        public string genorcldicReport(dbSelectlist list)
        {
            DataTable dt = new DataTable();
            OracleConnection con = new OracleConnection();
            string orclQuery = "";
            double totalCount = 0.0;
            string connStr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=" + list.Host + " )(PORT=" + list.Port + "))(CONNECT_DATA=(SID=" + list.Sid + ")));User Id=" + list.Userid + ";Password=" + list.Pass + "";
            try
            {
                dt = new DataTable();
                orclQuery = " Select Count(*) AS Support From " + list.Table + "";
                con.ConnectionString = connStr;
                using (OracleCommand cmd = new OracleCommand(orclQuery, con))
                {
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dt);
                    con.Close();
                }
                if (dt.Rows.Count > 0)
                {
                    totalCount = Convert.ToDouble(dt.Rows[0]["Support"].ToString());
                    string[] selCol = list.selCol.Split(',');
                    string cselCol = "";
                    if (selCol.Count() > 1)
                        foreach (string st in selCol)
                        {
                            cselCol = cselCol + (cselCol == "" ? "where C." + st + "= A." + st : " AND C." + st + "= A." + st);
                        }
                    dt = new DataTable();
                    orclQuery = " Select ROUND(Count(*)/" + totalCount + ",3) AS SUPPORT," +
                                " ROUND(((count(*)/" + totalCount + ") / ((select  Count(*) From " + list.Table + " B where B." + list.selAvgCol + " = A." + list.selAvgCol + ")/" + totalCount + "))" +
                                " /((select  Count(*) From " + list.Table + " C " + cselCol + ")/" + totalCount + "),3)  AS LIFT, " +
                                " ROUND((count(*)/" + totalCount + ") / ((select  Count(*) From " + list.Table + " D where D." + list.selAvgCol + " = A." + list.selAvgCol + ")/" + totalCount + "),3)  AS CONDITIONAL_PROBABILITY, " +
                                "" + list.selAvgCol + ", " + list.selCol + "  From " + list.Table + " A group by " + list.selAvgCol + ", " + list.selCol + "" +
                                " order by " + list.selAvgCol + ", " + list.selCol + "";
                    con.ConnectionString = connStr;
                    using (OracleCommand cmd = new OracleCommand(orclQuery, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        con.Open();
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);
                        con.Close();
                        con.Dispose();
                    }
                    return DTtoJSON(dt);
                }
                else
                {
                    con.Close();
                    con.Dispose();
                    return "Error : No record found";
                }
            }
            catch (Exception ex)
            {
                con.Close();
                con.Dispose();
                return "Error : " + ex.Message;
            }
        }





        // Internal functions
        public List<excelList> opencsv(string fileName)
        {
            List<excelList> list = new List<excelList>();
            try
            {
                string filepath = Server.MapPath("~/Fileupload/" + fileName);
                FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(936));
                string readLine = sr.ReadLine();
                if (readLine.Trim().Length > 0)
                {
                    string[] csvHead = readLine.Split(',');
                    for (int i = 0; i < csvHead.Count(); i++)
                    {
                        excelList excellist = new excelList();
                        excellist.key = "Head";
                        excellist.vale = csvHead[i].ToString();
                        excellist.fileName = fileName;
                        sr.Close();
                        list.Add(excellist);
                    }
                }
                else
                {
                    excelList excellist = new excelList();
                    excellist.key = "Error";
                    excellist.vale = "No columns found";
                    excellist.fileName = fileName;
                    list.Add(excellist);
                }
                return list;
            }
            catch (Exception ex)
            {
                excelList excellist = new excelList();
                excellist.key = "Error";
                excellist.vale = ex.ToString();
                excellist.fileName = fileName;
                list.Add(excellist);
                return list;
            }
        }

        public List<excelList> openxl(string fileName)
        {
            List<excelList> list = new List<excelList>();
            try
            {
                string conString = getxlConstr(fileName);
                OleDbConnection connExcel = new OleDbConnection(conString);
                OleDbCommand cmdExcel = new OleDbCommand();
                //OleDbDataAdapter oda = new OleDbDataAdapter();
                DataTable dt = new DataTable();
                DataTable dtExcelSchema;
                cmdExcel.Connection = connExcel;

                //Get the name of Sheets
                connExcel.Open();
                dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                connExcel.Close();

                if (dtExcelSchema.Rows.Count > 1)
                    foreach (DataRow dr in dtExcelSchema.Rows)
                    {
                        excelList excellist = new excelList();
                        excellist.key = "Sheet";
                        excellist.vale = dr["TABLE_NAME"].ToString();
                        excellist.fileName = fileName;
                        list.Add(excellist);
                    }
                else
                    list = selectxlhead(fileName, dtExcelSchema.Rows[0]["TABLE_NAME"].ToString());

                return list;
            }
            catch (Exception ex)
            {
                excelList excellist = new excelList();
                excellist.key = "Error";
                excellist.vale = ex.ToString();
                excellist.fileName = fileName;
                list.Add(excellist);
                return list;
            }
        }

        public string getxlConstr(string fileName)
        {
            string conStr = "";
            string filepath = Server.MapPath("~/Fileupload/" + fileName);
            string[] file = fileName.Split('.');
            switch (file[1].ToLower())
            {
                case "xls": //Excel 97-03
                    conStr = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                    break;

                case "xlsx": //Excel 07
                    conStr = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                    break;
            }
            conStr = String.Format(conStr, filepath, true);
            return conStr;
        }

        public List<excelList> selectxlhead(string fileName, string sheetName)
        {
            List<excelList> list = new List<excelList>();
            try
            {
                string conString = getxlConstr(fileName);
                OleDbConnection connExcel = new OleDbConnection(conString);
                OleDbCommand cmdExcel = new OleDbCommand();
                OleDbDataAdapter oda = new OleDbDataAdapter();
                DataTable dt = new DataTable();
                cmdExcel.Connection = connExcel;

                //Read Head Data from First Sheet
                connExcel.Open();
                cmdExcel.CommandText = "SELECT TOP 1 * From [" + sheetName + "]";
                oda.SelectCommand = cmdExcel;
                oda.Fill(dt);
                connExcel.Close();

                if (dt.Rows.Count > 0)
                    foreach (DataColumn dc in dt.Columns)
                    {
                        excelList excellist = new excelList();
                        excellist.key = "Head";
                        excellist.vale = dc.ToString();
                        excellist.DType = dc.DataType.ToString().ToLower();
                        excellist.fileName = fileName;
                        list.Add(excellist);
                    }
                else
                {
                    excelList excellist = new excelList();
                    excellist.key = "Error";
                    excellist.vale = "No columns found";
                    excellist.fileName = fileName;
                    list.Add(excellist);
                }
                return list;
            }
            catch (Exception ex)
            {
                excelList excellist = new excelList();
                excellist.key = "Error";
                excellist.vale = ex.ToString();
                excellist.fileName = fileName;
                list.Add(excellist);
                return list;
            }
        }

        public string DTtoJSON(DataTable table)
        {
            try
            {
                string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(table);
                //string fileLocation = System.Web.HttpContext.Current.Server.MapPath("~/Fileupload/");
                Random random = new Random();
                string fileName = ((DateTime.Now).ToString("ss yy mm  dd HH MM")).Replace(" ", string.Empty) + (random.Next(100, 999)).ToString();
                if (File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Fileupload/") + fileName + ".txt"))
                {
                    //File.Delete(fileName);
                    fileName = fileName + (random.Next(10, 99)).ToString();
                }
                using (FileStream fs = File.Create(System.Web.HttpContext.Current.Server.MapPath("~/Fileupload/") + fileName + ".txt"))
                {
                    Byte[] title = new UTF8Encoding(true).GetBytes(JSONString);
                    fs.Write(title, 0, title.Length);
                }

                return "Fileupload/" + fileName + ".txt";
            }
            catch (Exception ex)
            {
                return "Error : " + ex.Message;
            }
        }

        public List<excelList> cntOracletable(dbList list)
        {
            List<excelList> xllist = new List<excelList>();
            try
            {
                OracleConnection con = new OracleConnection();
                DataTable dt = new DataTable();
                string connStr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=" + list.Host + " )(PORT=" + list.Port + "))(CONNECT_DATA=(SID=" + list.Sid + ")));User Id=" + list.Userid + ";Password=" + list.Pass + "";
                string orclQuery = "select col.column_id, col.column_name, col.data_type from sys.dba_tab_columns col "
                                  + " inner join sys.dba_tables t on col.owner = t.owner and col.table_name = t.table_name "
                                  + " where col.table_name = '" + list.Table + "'";
                con.ConnectionString = connStr;
                using (OracleCommand cmd = new OracleCommand(orclQuery, con))
                {
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dt);
                    con.Close();
                    con.Dispose();
                }
                if (dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                    {
                        excelList excellist = new excelList();
                        excellist.key = "Head";
                        excellist.vale = dr[1].ToString();
                        excellist.DType = dr[2].ToString().ToLower();
                        excellist.fileName = list.Table;
                        xllist.Add(excellist);
                    }
                else
                {
                    excelList excellist = new excelList();
                    excellist.key = "Error";
                    excellist.vale = "No columns found";
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

        //Convert DataTable to IEnumerable
        private static IEnumerable<double> executeReader(DataTable reader, int column)
        {
            return reader.AsEnumerable().Select(row => row[column] == System.DBNull.Value ? 0 : Convert.ToDouble(row[column]));
        }

        private static Vector<Complex>   MultipleLinearRegression(Complex[][] x, double[] y) //Tuple<double[], double>
        {
            // Perform multiple linear regression using Math.NET
            //var result = MultipleRegression.NormalEquations(x, y);
            //Tuple<double[], double> result = MultipleRegression.NormalEquations(x, y);
            //var result = MultipleRegression.NormalEquations(DenseMatrix.OfRowArrays(x), y);


            Complex[] yVal = new Complex[y.Length];
            for (int i = 0; i < y.Length; i++)
            {
                yVal[i] = y[i];
            }
            DenseMatrix X = DenseMatrix.OfRowArrays(x);
            DenseVector Y = DenseVector.OfArray(yVal);

            Vector<Complex> result = MultipleRegression.NormalEquations(DenseMatrix.OfRowArrays(x), Y);

            Console.WriteLine(result);

            return result;
        }




        public class MultipleLinearRegressions
        {
            public static double[] Fit(double[][] factors, double[] predictor, bool intercept = true)
            {
                var designMatrix = Matrix<double>.Build.Dense(factors.Length, factors[0].Length + (intercept ? 1 : 0));
                for (int i = 0; i < factors.Length; i++)
                {
                    for (int j = 0; j < factors[i].Length; j++)
                    {
                        designMatrix[i, j] = factors[i][j];
                    }

                    if (intercept)
                    {
                        designMatrix[i, factors[i].Length] = 1.0;
                    }
                }

                //var coefficients =DenseMatrix.Solve(designMatrix, predictor);
                return null;// coefficients.ToArray();
            }
        }

        public static void LinearRegression(
           double[] xVals,
           double[] yVals,
           out double rSquared,
           out double yIntercept,
           out double slope)
        {
            if (xVals.Length != yVals.Length)
            {
                throw new Exception("Input values should be with the same length.");
            }



            //var xdata = new double[] { 10, 20, 30, 40, 50 };
            //var ydata = new double[] { 15, 20, 25, 55, 95 };

            double[] x1 = { 446, 2462, 8128, 1505, 6368, 1454, 861, 2342, 6445, 2439, 12151, 630, 1267, 13214, 1533, 5889, 14436, 1006, 5480, 6766, 2812, 4111, 6467, 7583, 3748, 1806, 5800, 4908, 2126, 6774, 2632, 18816, 1568, 3396, 7836, 1702, 3459, 4631, 2493, 1099, 5354, 7214, 4153, 2274, 2764, 5907, 2873, 1647, 3668, 1030, 8343, 3120, 1236, 4055, 866, 14427, 8197, 1582, 1719, 5639, 2690, 1031, 1919, 2899, 1026, 4864, 2999, 857, 5353, 1873, 461, 3850, 4207, 2646, 3439, 1413, 2991, 3292, 4263, 3594, 1905, 5228, 8171, 1317, 4148, 1854, 2701, 1739, 5015, 7449, 5278, 6458, 4862, 1453, 1824, 2878, 1106, 3776, 4696, 3411, 4981, 6873, 3771, 1093, 6901, 10227, 1046, 6742, 3857, 4200, 5559, 934, 4845, 1404, 1651, 1043, 2746, 1704, 1318, 2874, 772, 11600, 3328, 1102, 4892, 4340, 2394, 9069, 2445, 1787, 2680, 4082, 2073, 1974, 1451, 7106, 4970, 4911, 1789, 2577, 6376, 5528, 3887, 6111, 2407, 4915, 526, 24399, 2398, 3064, 2520, 1676, 3938, 5062, 1941, 6683, 4542, 10513, 1159, 3728, 2148, 4147, 2784, 1299, 1630, 432, 543, 2144, 6564, 30579, 18314, 984, 3794, 1671, 2358, 7811, 4853, 11106, 1875, 5820, 3848, 5018, 8366, 6161, 10651, 3753, 720, 1752, 4019, 2258, 2716, 2170, 6339, 3374, 4820, 2592, 2856, 3395, 2892, 2729, 9388, 3641, 2819, 2336, 4252, 6891, 2129, 3291, 13551, 6213, 865, 1613, 1802, 3125, 8205, 2153, 2917, 7113, 448, 4550, 4079, 5041, 3653, 6381, 5885, 2682, 2586, 1703, 1898, 2084, 2483, 1993, 7617, 659, 372, 4074, 2256, 6023, 1468, 19007, 1918, 3869, 3445, 1317, 3649, 1826, 14229 };
            double[] x2 = { 1089, 2468, 1739, 967, 2301, 492, 509, 497, 3353, 2277, 2787, 767, 1301, 36103, 2083, 810, 2508, 1806, 3700, 6638, 3749, 12143, 1712, 7805, 876, 1097, 1550, 17717, 2503, 1764, 869, 25003, 809, 3032, 7030, 779, 2301, 1370, 2118, 2404, 3079, 9650, 2981, 1005, 733, 10816, 828, 505, 3645, 1008, 25019, 1353, 2130, 4312, 1792, 3637, 16106, 1593, 981, 10046, 338, 614, 1560, 3509, 1103, 3483, 1119, 1672, 1337, 2170, 850, 1641, 4222, 770, 3201, 812, 3201, 2124, 5510, 282, 2102, 1453, 10141, 359, 1426, 957, 1906, 2369, 4704, 4494, 1469, 1072, 3241, 572, 2372, 6734, 706, 3301, 2887, 2003, 3277, 2317, 838, 430, 1505, 4805, 1540, 1340, 3177, 2871, 1680, 1574, 5128, 2157, 3279, 1424, 1208, 3010, 832, 5746, 2401, 14818, 4726, 2007, 10036, 3926, 1466, 12409, 2619, 1304, 1041, 2456, 964, 2644, 1650, 12401, 1777, 1686, 1388, 206, 10159, 2959, 1639, 2006, 1451, 2702, 695, 38701, 2925, 2624, 2731, 1124, 1353, 6001, 2251, 6572, 27670, 5935, 1915, 542, 499, 1101, 527, 753, 1150, 1403, 1097, 3262, 4153, 7211, 14084, 667, 660, 382, 532, 1277, 1004, 5872, 3018, 1866, 6871, 563, 15616, 3753, 12370, 622, 1081, 6384, 2343, 1525, 2205, 2194, 2200, 1373, 3015, 2806, 1001, 1035, 1608, 3449, 7320, 5953, 1360, 742, 6048, 7816, 2802, 4261, 1964, 1326, 983, 1853, 2108, 4002, 1644, 1002, 2058, 1733, 557, 1600, 4498, 3100, 2312, 12003, 4220, 3152, 440, 3267, 1512, 3482, 2859, 3106, 3674, 1458, 808, 3720, 2805, 10020, 1573, 20171, 2045, 5313, 5263, 295, 2790, 1761, 4090 };
            double[] x3 = { 249, 528, 333, 247, 548, 155, 95, 151, 924, 332, 271, 271, 185, 2189, 330, 133, 393, 500, 730, 907, 528, 1005, 318, 528, 287, 257, 335, 2094, 467, 404, 134, 3205, 209, 399, 745, 219, 278, 311, 539, 346, 561, 1047, 219, 108, 105, 950, 186, 165, 897, 231, 1868, 432, 493, 599, 287, 941, 1194, 321, 157, 937, 89, 129, 421, 665, 284, 880, 219, 459, 303, 588, 133, 247, 604, 243, 337, 216, 480, 352, 540, 31, 558, 362, 920, 44, 219, 162, 260, 523, 705, 836, 338, 270, 460, 150, 482, 846, 207, 546, 436, 316, 460, 595, 160, 100, 398, 1102, 251, 303, 416, 272, 383, 237, 706, 425, 617, 296, 387, 487, 229, 626, 561, 931, 874, 268, 773, 971, 314, 940, 356, 139, 121, 499, 180, 366, 377, 1007, 397, 314, 330, 69, 766, 577, 228, 297, 203, 288, 103, 2487, 782, 649, 550, 267, 286, 527, 656, 903, 1756, 1064, 321, 75, 121, 159, 181, 225, 376, 281, 205, 499, 530, 772, 1387, 298, 239, 70, 130, 287, 131, 939, 291, 469, 957, 56, 1233, 364, 746, 144, 282, 273, 273, 266, 396, 427, 711, 247, 625, 623, 120, 269, 250, 438, 908, 671, 260, 132, 456, 710, 459, 402, 286, 233, 115, 385, 529, 505, 477, 238, 225, 386, 152, 412, 505, 698, 411, 1260, 321, 442, 107, 903, 116, 244, 701, 390, 470, 249, 231, 661, 820, 715, 361, 1275, 532, 451, 662, 88, 501, 580, 362 };
            double[] x4 = { 242.86, 650.957, 382.184, 133.069, 950.443, 305.672, 156.612, 268.633, 964.879, 492.2, 766, 288.249, 322.6, 6256, 554.8, 115.275, 1040.683, 761.421, 750.793, 2104.66, 1382.719, 3437.036, 619, 1515.369, 335, 448.587, 202.269, 6336, 890.738, 279.684, 217.327, 8370.935, 191.722, 581.945, 1149.344, 347, 428.467, 815, 439.437, 657, 1243.9, 3917.4, 850.4, 41.999, 155.007, 3428, 207, 287.1, 1282.176, 333.194, 3198.405, 427.409, 871.5, 1719.5, 860.863, 1856, 3453.726, 403.23, 306.798, 2540.59, 57, 36.103, 329.184, 1203.9, 513.941, 1371.581, 372.757, 1223.996, 424.895, 305.117, 246.9, 318.853, 1711, 327, 1005, 74.73, 1046.526, 711.5, 1693.925, 77.983, 717.831, 647.684, 3025.326, 92.676, 502, 269.16, 480.399, 296.436, 2237.017, 1735.33, 679, 387, 741.507, 219, 802.1, 3285.1, 288.6, 585.505, 1197.6, 558.303, 780.05, 1196.802, 238, 164.061, 733, 2881.619, 487.116, 186.868, 796.307, 530.005, 935.034, 442, 1404, 497.254, 993.4, 432.341, 7240, 858.62, 211.11, 1480, 626.083, 3065.4, 1761.627, 371.4, 1923.379, 1265.247, 341.8, 3165.772, 314.3, 219.783, 241.198, 785.416, 308.3, 495.9, 819, 3537.988, 868.6, 347.029, 946.2, 53.876, 1962, 1401.2, 650.243, 867.1, 379.7, 381.1, 132.956, 5014.8, 1240.358, 849.8, 1038.5, 377.5, 559.7, 1817.032, 1240.836, 2450, 3726.797, 3270, 224.361, 107.3, 115.6, 118.127, 321.148, 489.046, 70.191, 320.091, 155.335, 811.984, 1794.4, 2138, 3892.3, 357.7, 190.45, 117.5, 273.963, 327.483, 230.327, 2648.804, 877.318, 667.837, 2741, 206.6, 3928.618, 694.884, 2199.342, 300, 365.6, 1467.2, 472.2, 313.4, 487.556, 190.988, 1412, 172.383, 1343.1, 852, 208.898, 404, 519.575, 763.534, 1666.243, 1143.089, 528.678, 270, 870.734, 2105.6, 540, 872.7, 758.8, 372, 169.357, 999.4, 961.809, 1243.726, 913, 453.3, 987, 924.328, 444.717, 328.311, 669.4, 928.6, 694.5, 4706, 723.1, 1392.334, 122.6, 1109.324, 223.702, 830.656, 835.083, 934, 1207.608, 301.913, 217.926, 506.555, 1109.501, 1555, 479.7, 591.532, 4430.884, 2104.6, 837.104, 85.534, 937.41, 669.4, 875.707 };
            //double[] y = { 3, 6, 8, 10, 13 };

            //// Combine independent variables into a 2D array
            //Complex[][] xVal = new Complex[x1.Length][];
            //for (int i = 0; i < x1.Length; i++)
            //{
            //    xVal[i] = new Complex[] { x1[i], x2[i] };//new Complex[Convert.ToInt32( x1[i]), Convert.ToInt32(x2[i])]; //( new Complex (x1[i]), x2[i] );
            //}

            //double[][] xVal = new double[x1.Length][];
            //for (int i = 0; i < x1.Length; i++)
            //{
            //    xVal[i] = new[] { x1[i], x2[i] };
            //}


            //double[][] x = new double[x1.Length][];
            //for (int i = 0; i < x1.Length; i++)
            //{
            //    x[i][i] = new double[x1[i]][x2[i]];//new Complex[Convert.ToInt32( x1[i]), Convert.ToInt32(x2[i])]; //( new Complex (x1[i]), x2[i] );
            //}
            //double[][] xVal = new double[][] { x };

            double[,] xVal = {
                { 446, 2462, 8128, 1505, 6368, 1454, 861, 2342, 6445, 2439, 12151, 630, 1267, 13214, 1533, 5889, 14436, 1006, 5480, 6766, 2812, 4111, 6467, 7583, 3748, 1806, 5800, 4908, 2126, 6774, 2632, 18816, 1568, 3396, 7836, 1702, 3459, 4631, 2493, 1099, 5354, 7214, 4153, 2274, 2764, 5907, 2873, 1647, 3668, 1030, 8343, 3120, 1236, 4055, 866, 14427, 8197, 1582, 1719, 5639, 2690, 1031, 1919, 2899, 1026, 4864, 2999, 857, 5353, 1873, 461, 3850, 4207, 2646, 3439, 1413, 2991, 3292, 4263, 3594, 1905, 5228, 8171, 1317, 4148, 1854, 2701, 1739, 5015, 7449, 5278, 6458, 4862, 1453, 1824, 2878, 1106, 3776, 4696, 3411, 4981, 6873, 3771, 1093, 6901, 10227, 1046, 6742, 3857, 4200, 5559, 934, 4845, 1404, 1651, 1043, 2746, 1704, 1318, 2874, 772, 11600, 3328, 1102, 4892, 4340, 2394, 9069, 2445, 1787, 2680, 4082, 2073, 1974, 1451, 7106, 4970, 4911, 1789, 2577, 6376, 5528, 3887, 6111, 2407, 4915, 526, 24399, 2398, 3064, 2520, 1676, 3938, 5062, 1941, 6683, 4542, 10513, 1159, 3728, 2148, 4147, 2784, 1299, 1630, 432, 543, 2144, 6564, 30579, 18314, 984, 3794, 1671, 2358, 7811, 4853, 11106, 1875, 5820, 3848, 5018, 8366, 6161, 10651, 3753, 720, 1752, 4019, 2258, 2716, 2170, 6339, 3374, 4820, 2592, 2856, 3395, 2892, 2729, 9388, 3641, 2819, 2336, 4252, 6891, 2129, 3291, 13551, 6213, 865, 1613, 1802, 3125, 8205, 2153, 2917, 7113, 448, 4550, 4079, 5041, 3653, 6381, 5885, 2682, 2586, 1703, 1898, 2084, 2483, 1993, 7617, 659, 372, 4074, 2256, 6023, 1468, 19007, 1918, 3869, 3445, 1317, 3649, 1826, 14229 },
                { 1089, 2468, 1739, 967, 2301, 492, 509, 497, 3353, 2277, 2787, 767, 1301, 36103, 2083, 810, 2508, 1806, 3700, 6638, 3749, 12143, 1712, 7805, 876, 1097, 1550, 17717, 2503, 1764, 869, 25003, 809, 3032, 7030, 779, 2301, 1370, 2118, 2404, 3079, 9650, 2981, 1005, 733, 10816, 828, 505, 3645, 1008, 25019, 1353, 2130, 4312, 1792, 3637, 16106, 1593, 981, 10046, 338, 614, 1560, 3509, 1103, 3483, 1119, 1672, 1337, 2170, 850, 1641, 4222, 770, 3201, 812, 3201, 2124, 5510, 282, 2102, 1453, 10141, 359, 1426, 957, 1906, 2369, 4704, 4494, 1469, 1072, 3241, 572, 2372, 6734, 706, 3301, 2887, 2003, 3277, 2317, 838, 430, 1505, 4805, 1540, 1340, 3177, 2871, 1680, 1574, 5128, 2157, 3279, 1424, 1208, 3010, 832, 5746, 2401, 14818, 4726, 2007, 10036, 3926, 1466, 12409, 2619, 1304, 1041, 2456, 964, 2644, 1650, 12401, 1777, 1686, 1388, 206, 10159, 2959, 1639, 2006, 1451, 2702, 695, 38701, 2925, 2624, 2731, 1124, 1353, 6001, 2251, 6572, 27670, 5935, 1915, 542, 499, 1101, 527, 753, 1150, 1403, 1097, 3262, 4153, 7211, 14084, 667, 660, 382, 532, 1277, 1004, 5872, 3018, 1866, 6871, 563, 15616, 3753, 12370, 622, 1081, 6384, 2343, 1525, 2205, 2194, 2200, 1373, 3015, 2806, 1001, 1035, 1608, 3449, 7320, 5953, 1360, 742, 6048, 7816, 2802, 4261, 1964, 1326, 983, 1853, 2108, 4002, 1644, 1002, 2058, 1733, 557, 1600, 4498, 3100, 2312, 12003, 4220, 3152, 440, 3267, 1512, 3482, 2859, 3106, 3674, 1458, 808, 3720, 2805, 10020, 1573, 20171, 2045, 5313, 5263, 295, 2790, 1761, 4090},
                { 249, 528, 333, 247, 548, 155, 95, 151, 924, 332, 271, 271, 185, 2189, 330, 133, 393, 500, 730, 907, 528, 1005, 318, 528, 287, 257, 335, 2094, 467, 404, 134, 3205, 209, 399, 745, 219, 278, 311, 539, 346, 561, 1047, 219, 108, 105, 950, 186, 165, 897, 231, 1868, 432, 493, 599, 287, 941, 1194, 321, 157, 937, 89, 129, 421, 665, 284, 880, 219, 459, 303, 588, 133, 247, 604, 243, 337, 216, 480, 352, 540, 31, 558, 362, 920, 44, 219, 162, 260, 523, 705, 836, 338, 270, 460, 150, 482, 846, 207, 546, 436, 316, 460, 595, 160, 100, 398, 1102, 251, 303, 416, 272, 383, 237, 706, 425, 617, 296, 387, 487, 229, 626, 561, 931, 874, 268, 773, 971, 314, 940, 356, 139, 121, 499, 180, 366, 377, 1007, 397, 314, 330, 69, 766, 577, 228, 297, 203, 288, 103, 2487, 782, 649, 550, 267, 286, 527, 656, 903, 1756, 1064, 321, 75, 121, 159, 181, 225, 376, 281, 205, 499, 530, 772, 1387, 298, 239, 70, 130, 287, 131, 939, 291, 469, 957, 56, 1233, 364, 746, 144, 282, 273, 273, 266, 396, 427, 711, 247, 625, 623, 120, 269, 250, 438, 908, 671, 260, 132, 456, 710, 459, 402, 286, 233, 115, 385, 529, 505, 477, 238, 225, 386, 152, 412, 505, 698, 411, 1260, 321, 442, 107, 903, 116, 244, 701, 390, 470, 249, 231, 661, 820, 715, 361, 1275, 532, 451, 662, 88, 501, 580, 362 }//,
                //{242.86, 650.957, 382.184, 133.069, 950.443, 305.672, 156.612, 268.633, 964.879, 492.2, 766, 288.249, 322.6, 6256, 554.8, 115.275, 1040.683, 761.421, 750.793, 2104.66, 1382.719, 3437.036, 619, 1515.369, 335, 448.587, 202.269, 6336, 890.738, 279.684, 217.327, 8370.935, 191.722, 581.945, 1149.344, 347, 428.467, 815, 439.437, 657, 1243.9, 3917.4, 850.4, 41.999, 155.007, 3428, 207, 287.1, 1282.176, 333.194, 3198.405, 427.409, 871.5, 1719.5, 860.863, 1856, 3453.726, 403.23, 306.798, 2540.59, 57, 36.103, 329.184, 1203.9, 513.941, 1371.581, 372.757, 1223.996, 424.895, 305.117, 246.9, 318.853, 1711, 327, 1005, 74.73, 1046.526, 711.5, 1693.925, 77.983, 717.831, 647.684, 3025.326, 92.676, 502, 269.16, 480.399, 296.436, 2237.017, 1735.33, 679, 387, 741.507, 219, 802.1, 3285.1, 288.6, 585.505, 1197.6, 558.303, 780.05, 1196.802, 238, 164.061, 733, 2881.619, 487.116, 186.868, 796.307, 530.005, 935.034, 442, 1404, 497.254, 993.4, 432.341, 7240, 858.62, 211.11, 1480, 626.083, 3065.4, 1761.627, 371.4, 1923.379, 1265.247, 341.8, 3165.772, 314.3, 219.783, 241.198, 785.416, 308.3, 495.9, 819, 3537.988, 868.6, 347.029, 946.2, 53.876, 1962, 1401.2, 650.243, 867.1, 379.7, 381.1, 132.956, 5014.8, 1240.358, 849.8, 1038.5, 377.5, 559.7, 1817.032, 1240.836, 2450, 3726.797, 3270, 224.361, 107.3, 115.6, 118.127, 321.148, 489.046, 70.191, 320.091, 155.335, 811.984, 1794.4, 2138, 3892.3, 357.7, 190.45, 117.5, 273.963, 327.483, 230.327, 2648.804, 877.318, 667.837, 2741, 206.6, 3928.618, 694.884, 2199.342, 300, 365.6, 1467.2, 472.2, 313.4, 487.556, 190.988, 1412, 172.383, 1343.1, 852, 208.898, 404, 519.575, 763.534, 1666.243, 1143.089, 528.678, 270, 870.734, 2105.6, 540, 872.7, 758.8, 372, 169.357, 999.4, 961.809, 1243.726, 913, 453.3, 987, 924.328, 444.717, 328.311, 669.4, 928.6, 694.5, 4706, 723.1, 1392.334, 122.6, 1109.324, 223.702, 830.656, 835.083, 934, 1207.608, 301.913, 217.926, 506.555, 1109.501, 1555, 479.7, 591.532, 4430.884, 2104.6, 837.104, 85.534, 937.41, 669.4, 875.707 }
            };

            double[,] yVal = { { 78.71428571 }, { 46.7 }, { 54.9047619 }, { 62.25 }, { 67.4 }, { 47.38095238 }, { 36.5 }, { 69.33333333 }, { 71 }, { 112 }, { 92.7 }, { 71.4 }, { 61.80952381 }, { 57.6 }, { 52.8 }, { 40.72222222 }, { 78.61904762 }, { 57.61904762 }, { 81.47368421 }, { 49.42105263 }, { 80.75 }, { 67.73684211 }, { 53.75 }, { 61.85 }, { 53.33333333 }, { 41.3 }, { 64.4 }, { 92.75 }, { 50.5 }, { 55.95 }, { 48.5 }, { 75.5 }, { 65.28571429 }, { 52 }, { 40.05 }, { 46.57142857 }, { 37.42857143 }, { 66.05 }, { 99.47619048 }, { 86.35 }, { 81.45 }, { 37.9 }, { 79.4 }, { 67.19047619 }, { 57.42857143 }, { 40.9 }, { 55.76190476 }, { 44.25 }, { 36.31578947 }, { 72.23809524 }, { 35.57894737 }, { 52 }, { 43.80952381 }, { 67.05263158 }, { 34.6 }, { 67.4 }, { 65.63157895 }, { 130.7619048 }, { 65.52380952 }, { 55.57894737 }, { 32.14285714 }, { 21.52380952 }, { 65.75 }, { 71.61904762 }, { 32.85714286 }, { 73.33333333 }, { 64.0952381 }, { 74.61904762 }, { 50.85 }, { 55.75 }, { 37.71428571 }, { 60.9 }, { 43.57894737 }, { 68.71428571 }, { 56.6 }, { 25.38095238 }, { 38.47368421 }, { 59.05 }, { 63.47368421 }, { 37.95 }, { 54.1 }, { 74.25 }, { 67.25 }, { 48.15 }, { 53.1 }, { 55.1 }, { 88.78947368 }, { 57.89473684 }, { 62.72222222 }, { 72.7 }, { 61.45 }, { 55.45 }, { 47.8 }, { 31.7 }, { 68.31578947 }, { 43.1 }, { 60.25 }, { 70.25 }, { 92.15 }, { 81.85 }, { 64.95 }, { 80.95 }, { 51.7 }, { 36.85 }, { 120.2 }, { 95.25 }, { 42.68421053 }, { 45.75 }, { 43.2 }, { 41.8 }, { 68.3 }, { 70.8 }, { 60.3 }, { 82.15 }, { 59.55 }, { 74.25 }, { 36 }, { 89.4 }, { 38.25 }, { 64.95 }, { 113.8 }, { 54.31578947 }, { 62 }, { 34.8 }, { 65.05555556 }, { 78.15 }, { 92.6 }, { 63.84210526 }, { 50.3 }, { 38.95 }, { 131.95 }, { 62 }, { 80.15789474 }, { 47.85 }, { 68.15 }, { 66.8 }, { 32.75 }, { 40.78947368 }, { 58.25 }, { 39.15 }, { 42.35 }, { 79.7 }, { 51.15 }, { 88.55 }, { 81.7 }, { 47.7 }, { 44.05 }, { 55.52631579 }, { 63.85 }, { 51.85 }, { 45.1 }, { 40.7 }, { 31.7 }, { 63.26315789 }, { 103.7 }, { 32.85 }, { 47.2 }, { 69.88888889 }, { 55.45 }, { 117.95 }, { 26.3 }, { 68.8 }, { 47.55 }, { 79.05 }, { 62.6 }, { 81.25 }, { 61 }, { 58.1 }, { 72.7 }, { 72.55555556 }, { 73.68421053 }, { 101.4 }, { 45.45 }, { 84.7 }, { 38.1 }, { 50.3 }, { 57.7 }, { 84.45 }, { 55.3 }, { 97.55 }, { 60.72222222 }, { 55.85 }, { 61.95 }, { 58.25 }, { 58.2 }, { 42.8 }, { 55.55 }, { 38.3 }, { 75.8 }, { 59.55 }, { 124.35 }, { 59.15 }, { 67.65 }, { 76.6 }, { 69.1 }, { 56.6 }, { 63 }, { 79.4 }, { 45.65 }, { 73.45 }, { 66.85 }, { 58.55 }, { 50.57894737 }, { 59.31578947 }, { 62.05 }, { 71.6 }, { 75.35 }, { 113.7894737 }, { 69.57894737 }, { 63.05263158 }, { 40.78947368 }, { 86.52631579 }, { 62.94736842 }, { 58.73684211 }, { 97.77777778 }, { 52.57894737 }, { 44.15789474 }, { 85.36842105 }, { 60.1 }, { 57.42105263 }, { 53.6 }, { 60.47368421 }, { 39.52380952 }, { 53.4 }, { 56.9 }, { 41.78947368 }, { 47 }, { 69.35 }, { 50.21052632 }, { 49.4 }, { 56.95 }, { 78.19047619 }, { 71.15 }, { 97.35 }, { 81.1 }, { 51.35 }, { 61.1205 }, { 45.23809524 }, { 42.5 }, { 59.05 }, { 51.68421053 }, { 59.21052632 }, { 60.55 }, { 43.16666667 }, { 56.63157895 }, { 48.05263158 }, { 55.52631579 }, { 1 } };

            //double[,] _X= { { 1, 2, 3},
            //                { 2, 9, 11},
            //                { 56, 111, 66}};

            //double[,] _Y = { { 6 }, { 6 }, { 11 } };

            var linearRegressor = new MultipleLinearRegressor();
            linearRegressor.Fit(xVal, yVal);




            double[][] inputs =
                                {
                                    new double[] { 1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,3,3,3,3,3,3,3,3,3,3,4,4,4,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,6,6,6,6,6,6,6,6,5,6,6,7,7,7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8,8,8,9,9,9,9,9,9,9,9,9,9,10,10,10,10,10,10,10,10,10,10 },
                                    new double[] {1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,3,3,3,3,3,3,3,3,3,3,4,4,4,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,6,6,6,6,6,6,6,6,5,6,6,7,7,7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8,8,8,9,9,9,9,9,9,9,9,9,9,10,10,10,10,10,10,10,10,20,20 },
                                    new double[] {  11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 55, 55, 55, 55, 55, 55, 55, 55, 55, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 77, 77, 77, 77, 77, 77, 77, 77, 77, 77, 88, 88, 88, 88, 88, 88, 88, 88, 88, 88, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 110, 110, 110, 110, 110, 110, 110, 110, 210, 210 }
                                    //x1,x2,x3,x4
                                };

            double[] outputs = { 1, 1, 1 };

            //Complex[] y = new Complex[yVal.Length];
            //for (int i = 0; i < yVal.Length; i++)
            //{
            //    y[i] = yVal[i];
            //}

            //DenseVector Y = DenseVector.OfArray(y);
            //var ols = new OrdinaryLeastSquares()
            //{
            //    UseIntercept = true
            //};
            //MultipleLinearRegression regression = ols.Learn(inputs, outputs);

            //double ur2 = regression.CoefficientOfDetermination(inputs, outputs, adjust: true);

            //var designMatrix = Matrix<double>.Build.DenseOfColumnArrays(inputs);

            //var designVector = Vector<double>.Build.DenseOfArray(yVal);

            //var regression = MultipleRegression.NormalEquations(designMatrix, designVector);
            //var coefficients = regression.Coefficients;

            //var prediction = regression[0] + regression[1] * inputs[1] + regression[2] * inputs[2] + regression[3] * inputs[3];

            //Complex[] yVal = new Complex[y.Length];
            //for (int i = 0; i < y.Length; i++)
            //{
            //    yVal[i] = new[] { y[i] };
            //}

            //double[] yVal = new double[] { y };

            // Perform multiple linear regression
            //Tuple<double[], double> result = MultipleLinearRegression(x, y);

            //var result = MultipleLinearRegression(xVal, yVal);


            //var X = DenseMatrix.CreateFromColumns(new[] { new DenseVector(xdata.Length, 1), new DenseVector(xdata) });
            //var y = new DenseVector(ydata);

            //var p = X.QR().Solve(y);
            //var a = p[0];
            //var b = p[1];
            //var X = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseOfColumnArrays(new[] {
            //        new Vector<double>..Dense(xdata.Length, 1),
            //        new DenseVector(xdata.Select(t => Math.Sin(omega*t)).ToArray()),
            //        new DenseVector(xdata.Select(t => Math.Cos(omega*t)).ToArray())});
            //var y = new DenseVector(ydata);

            //double[][] inputs = { xVals, yVals };
            //double[] output= { };
            //var outputs = Matrix<double>.Build.DenseOfColumnArrays(output);

            //var designMatrix =Matrix<double>.Build.DenseOfColumnArrays(inputs);
            //var x = Matrix<double>.Build.DenseOfColumnArrays(Vector<double>.Build.Dense(xVals.Length, 1.0), Vector<double>.Build.Dense(xVals));
            //    var regression = MultipleRegression.NormalEquations(Matrix<double>.Build.DenseOfColumnArrays(Vector<double>.Build.Dense(xVals.Length,1.0),Vector<double>.Build.Dense(xVals)), Matrix<double>.Build.DenseOfColumnArrays(Vector<double>.Build.Dense(yVals.Length, 1.0), Vector<double>.Build.Dense(yVals)));
            //var coefficients = regression.Coefficients;


            //Tuple"<"double, double">" p = Fit.Line(xVals, yVals);


            //double sumOfX = 0;
            //double sumOfY = 0;
            //double sumOfXSq = 0;
            //double sumOfYSq = 0;
            //double sumCodeviates = 0;

            //for (var i = 0; i < xVals.Length; i++)
            //{
            //    var x = xVals[i];
            //    var y = yVals[i];
            //    sumCodeviates += x * y;
            //    sumOfX += x;
            //    sumOfY += y;
            //    sumOfXSq += x * x;
            //    sumOfYSq += y * y;
            //}

            //var count = xVals.Length;
            //var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            //var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            //var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            //var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            //var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            //var meanX = sumOfX / count;
            //var meanY = sumOfY / count;
            //var dblR = rNumerator / Math.Sqrt(rDenom);

            //rSquared = dblR * dblR;
            //yIntercept = meanY - ((sCo / ssX) * meanX);
            //slope = sCo / ssX;

            rSquared = 0;
            yIntercept = 0;
            slope = 0;
        }

        public class MultipleLinearRegressor
        {
            private double _b;
            private double[] _w;

            public MultipleLinearRegressor()
            {
                _b = 0;
            }

            public void Fit(double[,] X, double[,] y)
            {
                var input = ExtendInputWithOnes(X);
                var output = Matrix<double>.Build.DenseOfArray(y);

                var coeficients = ((input.Transpose() * input).Inverse() * input.Transpose() * output)
                               .Transpose().Row(0);
                _b = coeficients.ElementAt(0);
                _w = SubArray(coeficients.ToArray(), 1, X.GetLength(1));
            }

            public double Predict(double[,] x)
            {
                var input = Matrix<double>.Build.DenseOfArray(x).Transpose();
                var w = Vector<double>.Build.DenseOfArray(_w);
                return input.Multiply(w).ToArray().Sum() + _b;
            }

            private Matrix<double> ExtendInputWithOnes(double[,] X)
            {
                // Add 'ones' to the input array to model coefficient b in data.
                var ones = Matrix<double>.Build.Dense(X.GetLength(0), 1, 1d);
                var extendedX = ones.Append(Matrix<double>.Build.DenseOfArray(X));

                return extendedX;
            }

            private double[] SubArray(double[] data, int index, int length)
            {
                double[] result = new double[length];
                Array.Copy(data, index, result, 0, length);
                return result;
            }
        }

    }





    public class allcombo
    {
        public List<Tuple<string, int>> GetCombination(List<excelList> list)
        {
            try
            {

                List<Tuple<string, int>> outlist = new List<Tuple<string, int>>();
                double count = Math.Pow(2, list.Count);
                int order = 0;
                for (int i = 1; i <= count - 1; i++)
                {
                    string outval = "";
                    order = 0;
                    string str = Convert.ToString(i, 2).PadLeft(list.Count, '0');
                    for (int j = 0; j < str.Length; j++)
                    {
                        if (str[j] == '1')
                        {
                            outval = outval + (outval.Trim().Length > 0 ? ", " : "") + list[j].vale;
                            order++;
                        }
                    }
                    outlist.Add(new Tuple<string, int>(outval, order));
                }
                return outlist.OrderBy(o => o.Item2).ToList();//outlist.Sort();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

}



//Excel All
//List<Ilist> ilist = new List<Ilist>();
//while (k < exlList.Count)
//{
//    if (!(ilist.Exists(x => x.vale == exlList[k - 1].vale)))
//    {
//        selCol = "";
//        for (int i = k; i < exlList.Count; i++)
//        {
//            if (i >= 1)
//            {
//                selCol = selCol + exlList[i - 1].vale + ",";
//                ilist.Add(new Ilist() { vale = exlList[i - 1].vale });
//            }

//            for (int j = i; j < exlList.Count; j++)
//            {
//                selCol1 = "";
//                if (exlList[j].vale != list.selAvgCol)
//                {
//                    selCol1 = selCol + (selCol1.Trim().Length > 1 ? (selCol1 + "," + exlList[j].vale) : exlList[j].vale);
//                    connExcel.Open();
//                    cmdExcel.CommandText = " SELECT format(Count(*)/" + totalCount + ",'0.000') AS SUPPORT," +
//                                           " format((" + Alift + "/(Avg (" + list.selAvgCol + "))), '0.00') AS LIFT, " + "format(( sum(" + list.selAvgCol + ") /(Count(*))), '0.000') AS CONFIDENCE," +
//                                           " format((Avg (" + list.selAvgCol + ")), '0.00') AS AVG_" + list.selAvgCol + ",  " + selCol1 +
//                                           " From [" + sheetName + "] GROUP BY " + selCol1;
//                    oda.SelectCommand = cmdExcel;
//                    oda.Fill(dt);
//                    connExcel.Close();
//                }

//            }
//        }
//    }
//    else
//        ilist = new List<Ilist>();
//    k = k + 1;
//}



//Oracle All
//string selCol = "", selCol1 = "";
//int k = 0;
//List<Ilist> ilist = new List<Ilist>();
//while (k < exlList.Count)
//{
//    if (!(ilist.Exists(x => x.vale == exlList[k - 1].vale)))
//    {
//        selCol = "";
//        ilist = new List<Ilist>();
//        for (int i = k; i < exlList.Count; i++)
//        {
//            if (i >= 1)
//            {
//                selCol = selCol + exlList[i - 1].vale + ",";
//                ilist.Add(new Ilist() { vale = exlList[i - 1].vale });
//            }


//            for (int j = i; j < exlList.Count; j++)
//            {
//                selCol1 = "";
//                if (exlList[j].vale != list.selAvgCol)
//                {
//                    selCol1 = selCol + (selCol1.Trim().Length > 1 ? (selCol1 + "," + exlList[j].vale) : exlList[j].vale);
//                    orclQuery = " Select  ROUND(Count(*)/" + totalCount + ",3) AS SUPPORT," +
//                                " ROUND((" + Alift + "/Avg(" + list.selAvgCol + ")),2) AS LIFT, " +
//                                " ROUND(Avg(" + list.selAvgCol + "),2) AS AVG_" + list.selAvgCol + ",  " + selCol1 +
//                                " from " + list.Table + "  group by " + selCol1 + " order by " + selCol1 + "";
//                    con.ConnectionString = connStr;
//                    using (OracleCommand cmd = new OracleCommand(orclQuery, con))
//                    {
//                        cmd.CommandType = CommandType.Text;
//                        con.Open();
//                        OracleDataAdapter da = new OracleDataAdapter(cmd);
//                        da.Fill(dt);
//                        con.Close();
//                    }

//                }

//            }
//        }
//    }
//    else
//        ilist = new List<Ilist>();
//    k = k + 1;
//}