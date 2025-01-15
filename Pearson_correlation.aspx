<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Pearson_correlation.aspx.cs" Inherits="Grand_Report.Pearson_correlation" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Pearson Correlation</title>
    <link href="../css/fonts/style.css" rel="stylesheet" />
    <link href="../css/fonts/font-awesome.min.css" rel="stylesheet" />
    <link href="../vendor/bootstrap/v3/bootstrap.min.css" rel="stylesheet" />
    <link href="../vendor/bootstrap/v4/bootstrap-grid.css" rel="stylesheet" />
    <link href="../css/style.css" rel="stylesheet" />
    <link href="//cdn.datatables.net/1.10.24/css/jquery.dataTables.min.css" rel="stylesheet" />
</head>
<body>
    <!-- HEADER -->
    <header>
        <div class="header js-header js-dropdown">
            <div class="container">
                <div class="header__user">
                    <div class="header__user-btn" id="divuser" data-dropdown-btn="user">
                        <a href="Index.aspx" class="tooltips"><i class="fa fa-file-text-o" aria-hidden="true">&nbsp;&nbsp;Grand Report</i></a>
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <a href="Pearson_correlation.aspx" class="tooltips"><i class="fa fa-area-chart" aria-hidden="true">&nbsp;&nbsp;Regression</i></a>
                    </div>
                </div>
            </div>
        </div>
    </header>

    <main>
        <div class="container">
            <form id="form1" runat="server">
                <div>

                    <div class="col-sm-12">
                        <div class="create__section">
                            <div class="row" style="padding: 2% 0;">
                                <div class="col-md-2">
                                    <label class="create__label" for="category">Select Source</label>
                                </div>
                                <div class="col-md-4">
                                    <select id="Select1" onchange="selectionchange(2)">
                                        <option>Select</option>
                                        <%--<option>Oracle</option>--%>
                                        <option>SQL Server</option>
                                        <option>Excel</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                    </div>


                    <div id="div_sql" class=" col-md-12 hide ">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Host">Data Source *</label>
                                    <input id="txt_Source" class="form-control" type="text" value="sql5103.site4now.net" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Port">Database *</label>
                                    <input id="txt_db" class="form-control" type="text" value="DB_A568A7_ssijopious" />
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Userid">User Id *</label>
                                    <input id="txt_SUserid" class="form-control" type="text" value="" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="pwd_Pass">Password *</label>
                                    <input id="pwd_SPass" class="form-control" type="password" value="" />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="create__footer">
                                <label class="create__btn-create btn btn--type-02" onclick="cntSql(null);">Connect</label>
                            </div>
                        </div>
                    </div>


                    <div id="div_orcl" class=" col-md-12 hide ">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Host">HOST *</label>
                                    <input id="txt_Host" class="form-control" type="text" value="localhost" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Port">PORT *</label>
                                    <input id="txt_Port" class="form-control" type="text" value="1521" />
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Sid">SID *</label>
                                    <input id="txt_Sid" class="form-control" type="text" value="orcl" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Table">Table *</label>
                                    <input id="txt_Table" class="form-control" type="text" value="DEMO_DATA" />
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="txt_Userid">User Id *</label>
                                    <input id="txt_Userid" class="form-control" type="text" value="sijo" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="pwd_Pass">Password *</label>
                                    <input id="pwd_Pass" class="form-control" type="password" value="pwd4sijo" />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="create__footer">
                                <label class="create__btn-create btn btn--type-02" onclick="cntOracle(null);">Connect</label>
                            </div>
                        </div>
                    </div>

                    <div id="div_excel" class="col-md-12 hide">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="create__section">
                                    <label class="create__label" for="fle_Excel">Select File</label>
                                    <input id="fle_Excel" type="file" accept=".xlsx, .xls" />
                                    <label class="hide" for="fle_Excel" id="lblflName"></label>
                                    <label class="hide" for="fle_Excel" id="lblcolnum">5</label>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="create__footer">
                                    <label class="create__btn-create btn btn--type-02" onclick="savefile(null);">Upload</label>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div id="div_sheetlist" class="hide">
                        <div class="col-md-12">
                            <div class="create__section">
                                <div class="row" style="padding: 2% 0;">
                                    <div class="col-md-2">
                                        <label class="create__label" for="sheetSelect1">Select Table</label>
                                    </div>
                                    <div class="col-md-4">
                                        <select id="sheetSelect1" onchange="getColumns('sheetSelect1')">
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div id="div_Headlist" class="col-md-4 hide">
                        </div>
                        <div id="div_Columnlist" class="col-md-8 hide">
                            <label class="hide" id="lblhCount">0</label>
                            <label class="create__btn-create btn btn--type-03" style="margin: 1% 0;" onclick="genCorrel();">Generate Correlation</label>
                            <table class='table  table-striped' id='tblrpt'>
                                <thead class='thead-light'>
                                    <tr>
                                        <th scope='col'>Target<br />
                                            <span>
                                                <input id='chkTrgAll' type='checkbox' name='chkTarget' onchange='chkChangetblall("chkTrgAll","chkCovAll","chkTarget","chkCovar");' value='All' /></span>  <span>All</span><input id="txtcolsearch0" class="form-control hide" type="text" value="" placeholder="Search" onchange="txtcolSearch()" /></th>
                                        <th scope='col'>Covariance<br />
                                            <span>
                                                <input id='chkCovAll' type='checkbox' name='chkCovar' onchange='chkChangetblall("chkCovAll","chkTrgAll","chkCovar","chkTarget");' value='All' /></span>  <span>All</span><input id="txtcolsearch1" class="form-control hide" type="text" value="" placeholder="Search" onchange="txtcolSearch()" /></th>
                                        <th scope='col'>Table Name
                                            <br />
                                            <input id="txtcolsearch2" class="form-control" type="text" value="" placeholder="Search" onchange="txtcolSearch()" /></th>
                                        <th scope='col'>Column Name<br />
                                            <input id="txtcolsearch3" class="form-control" type="text" value="" placeholder="Search" onchange="txtcolSearch()" /></th>
                                        <th scope='col'>Where<br />
                                            <span>&nbsp;</span><input id="txtcolsearch4" class="form-control hide" type="text" value="" placeholder="Search" onchange="txtcolSearch()" /></th>
                                        <th scope='col'>Group By<br />
                                            <span>&nbsp;</span><input id="txtcolsearch5" class="form-control hide" type="text" value="" placeholder="Search" onchange="txtcolSearch()" /></th>
                                    </tr>
                                </thead>
                                <tbody id="tbl_column">
                                </tbody>
                            </table>


                        </div>
                    </div>

                </div>
            </form>
        </div>
    </main>

    <!-- Footer -->
    <footer class="bg-light text-center text-lg-start">
        <!-- Grid container -->
        <div class="container p-4">
            <!--Grid row-->
            <div class="row">
                <!--Grid column-->
                <div class="col-md-12">
                    <h6 class="tooltips"><a href="Index.aspx" class="tooltips">Information Systems Group,</a>&nbsp;&nbsp;
                        <a href="https://taltech.ee/" class="tooltips">Tallinn University of Technology</a>
                    </h6>

                    <p>
                        <a href="Index.aspx" class="tooltips"><i class="fa fa-file-text-o" aria-hidden="true">&nbsp;&nbsp;Grand Report</i></a>
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <a href="Pearson_correlation.aspx" class="tooltips"><i class="fa fa-area-chart" aria-hidden="true">&nbsp;&nbsp;Regression</i></a>
                    </p>
                </div>
            </div>
        </div>
    </footer>

    <div id="popup1" class="overlay hide">
        <div class="popup">
            <h5>Pearson Correlation</h5>
            <label class="close" onclick="closePop()">&times;</label>
            <label id="lblxvar" style="font-weight:bold">Xvar</label>
            <div class="content">
                <table id="tblrptCor">
                    <thead>
                        <tr>
                            <th class="colleft">Column name</th>
                            <th class="colright">Correlation value</th>
                        </tr>
                    </thead>
                    <tbody id="tbdyCorr">
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <div class="col-lg-12 hover hide" id="divhover">
        <div class="hoverdiv" style="">
            <div class="loader"></div>
        </div>
    </div>
</body>
<script src="jquery/jquery-3.4.1.min.js"></script>
<script src="jquery/bootstrap.min.js"></script>
<script src="jquery/test.js"></script>
<script src="//cdn.datatables.net/1.10.24/js/jquery.dataTables.min.js"></script>
</html>
