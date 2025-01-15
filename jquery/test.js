function showhide(idx, mode) {
    var myButtonClasses = document.getElementById(idx).classList;
    if (myButtonClasses.contains("hide")) {
        myButtonClasses.remove("hide");
    }
    if (mode === "h")
        document.getElementById(idx).className += " hide ";
}

function selectionchange(ele) {
    showhide('divhover', 's');
    hideSheet_Head(ele);
    var seltxt = $("#Select1").select().val();
    switch (seltxt) {
        case "Oracle":
            showhide("div_orcl", "s");
            showhide("div_excel", "h");
            showhide("div_sql", "h");
            break;
        case "Excel":
            showhide("div_orcl", "h");
            showhide("div_excel", "s");
            showhide("div_sql", "h");
            break;
        case "SQL Server":
            showhide("div_orcl", "h");
            showhide("div_excel", "h");
            showhide("div_sql", "s");
            break;
        default:
            showhide("div_orcl", "h");
            showhide("div_excel", "h");
            showhide("div_sql", "h");
    }
    showhide('divhover', 'h');
}

function hideSheet_Head(ele) {
    if (ele == 1) {
        showhide("div_sheetlist", "h");
        showhide("div_Headlist", "h");
        showhide("div_reff", "h");
        $("#div_Headlist").empty();
        $("#sheetSelect1").empty();
        $("#div_Report").empty();
    }
    else if (ele == 2) {
        showhide("div_Headlist", "h");
        showhide("div_Columnlist", "h");
        $("#div_Headlist").empty();
        $("#tbl_column").empty();
    }
}

function savefile() {
    showhide('divhover', 's');
    hideSheet_Head();
    var files = $("#fle_Excel").get(0).files;
    var formData = new FormData();
    formData.append('file', files[0]);
    var choice = {};
    choice.url = "fileHandler.ashx";
    choice.type = "POST";
    choice.data = formData;
    choice.contentType = false;
    choice.processData = false;
    choice.success = function (result) {
        if (result == "filechk") {
            showhide('divhover', 'h');
            alert("Only .csv or .xls or .xlsx file are allowed");
        }
        else {
            fileDetail(result);
        }

    };
    choice.error = function (err) {
        showhide('divhover', 'h');
        alert(err.statusText);
    };
    $.ajax(choice);
    event.preventDefault();
}

function fileDetail(fileName) {
    $.ajax({
        type: "POST",
        url: "webControlls.aspx/webfileDetail",
        data: '{fileName: "' + fileName + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.d[0].key != "Error")
                if (response.d[0].key == "Sheet") {
                    fillSheet(response.d);
                    showhide('divhover', 'h');
                }
                else {
                    fillHead(response.d);
                    showhide('divhover', 'h');
                }
            else {
                alert(response.d[0].vale);
                showhide('divhover', 'h');
            }
        },
        failure: function (response) {
            alert(response.d[0].vale);
            showhide('divhover', 'h');
        }
    });
}

function fillSheet(ele) {
    var listtr = "";
    listtr = listtr + " <option >Select</option>";
    for (i = 0; i < ele.length; i++)
        if (ele[i].key == "Sheet")
            listtr = listtr + " <option id='" + i + "'>" + ele[i].vale + "</option>";
    $("#lblflName").text(ele[0].fileName);
    $("#sheetSelect1").empty();
    $("#sheetSelect1").append(listtr);
    showhide('divhover', 'h');
    showhide("div_sheetlist", "s");
}

function sheetChange(ele) {
    showhide('divhover', 's');
    var list = {
        vale: $("#" + ele).select().val()
        , fileName: $("#lblflName").text()
    };
    $.ajax({
        type: "POST",
        url: "webControlls.aspx/webgetHead",
        data: '{ "list":' + JSON.stringify(list) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.d[0].key != "Error") {
                fillHead(response.d);
                showhide('divhover', 'h');
            }
            else {
                alert(response.d[0].vale);
                showhide('divhover', 'h');
            }
        },
        failure: function (response) {
            alert(response.d[0].vale);
            showhide('divhover', 'h');
        }
    });
}

var numDtType = "byte, decimal , double, int16, int32, int64, sbyte, single, uint16, uint32, uint64, integer, long, currency, number, float";

function fillHead(ele) {
    var listtr = "";
    listtr = listtr + "  <div class='table'><table id=ulHeadList class='list'>"
        + " <thead><tr> <th>Target factor</th><th>Influence factor</th>  <th>Column name</th></tr></thead>"
        + " <tbody><tr> <td></td><td> <input id='chkColAll' type='checkbox' name='cheInflu' onchange='chkChangeall();' value='All'></td>  <td>All</td></tr>";//&nbsp;&nbsp;
    for (i = 0; i < ele.length; i++) {
        if (ele[i].key == "Head") {
            var dtType = ele[i].DType.split(".");
            if ((numDtType.includes(dtType[1])) == true || (numDtType.includes(dtType)) == true)
                listtr = listtr + " <tr> <td><input type='checkbox' id='rdoAvg" + i + "' name='rdoHeadNum' value='" + ele[i].vale + "' onchange='rdoAvgchange(" + i + ");'> </td>  <td><input id='chkCol" + i + "' type='checkbox' name='cheInflu' onchange='chkChange(" + i + ",0);' value='" + ele[i].vale + "'></td>  <td>" + ele[i].vale + " (Numeric)</td></tr>";
            else
                listtr = listtr + " <tr> <td><input type='checkbox' id='rdoAvg" + i + "' name='rdoHeadDis' value='" + ele[i].vale + "' onchange='rdoDicchange(" + i + ");'> </td>  <td><input id='chkCol" + i + "' type='checkbox' name='cheInflu' onchange='chkChange(" + i + ",0);' value='" + ele[i].vale + "'></td>  <td>" + ele[i].vale + "</td></tr>";
        }
    }
    $("#lblflName").text(ele[0].fileName);
    listtr = listtr + "</tbody> </table></div> <label class='create__btn-create btn btn--type-03' style='margin: 1% 0;' onclick='genReport(null);'>Report</label>";
    $("#div_Headlist").empty();
    $("#div_Headlist").append(listtr);
    showhide('divhover', 'h');
    showhide("div_Headlist", "s");
}

function rdoAvgchange(ele) {
    if ($('input[name="rdoHeadNum"]:checked').length > 2) {
        $("#rdoAvg" + ele).prop("checked", false);
        alert("You have selected maximum number of target column.");
    }
    else {
        $("#chkCol" + ele).prop("checked", false);
    }
    $("input[name='rdoHeadDis']").prop("checked", false);
    //alert($('input[name="rdoHeadNum"]:checked').length);

}

function rdoDicchange(ele) {
    $("#chkCol" + ele).prop("checked", false);
    $("input[name='rdoHeadNum']").prop("checked", false);
    $("input[name='rdoHeadDis']").prop("checked", false);
    $("#rdoAvg" + ele).prop("checked", true);
    $("#chkColAll").prop("checked", false);
}

function chkChange(ele, idele) {
    if (idele == 0) {
        $("#rdoAvg" + ele).prop("checked", false);
        $("#chkColAll").prop("checked", false);
    }
    else if (idele == 1) {
        $("#chkCovar" + ele).prop("checked", false);
        //$("#txt_" + ele).val("");
    }
    else if (idele == 2) {
        $("#chkTarget" + ele).prop("checked", false);
        //$("#txt_" + ele).val("");
    }
    //else if (idele == 3) {
    //    if ($("#txt_" + ele).val().length > 0) {
    //        $("#chkCovar" + ele).prop("checked", false);
    //        $("#chkTarget" + ele).prop("checked", false);
    //    }
    //}
}

function chkChangeall() {
    var colName = $("#ulHeadList").find("input:checkbox").map(function () {
        return this.value;
    }).get().toString().split(',');
    for (i = 1; i < colName.length; i++) {
        var k = i - 1;
        $("#chkCol" + k).prop("checked", false);
    }
    $("input[name='rdoHeadDis']").prop("checked", false);
}

function genReport() {

    
    showhide('divhover', 's');
    var seltxt = $("#Select1").select().val();
    var selFlag = "";
    var selVal = "";
    if ($("input[name='rdoHeadDis']:checked").val() != null) {
        selFlag = "dic";
        selVal = $("input[name='rdoHeadDis']:checked").val();
    }
    else {
        selFlag = "avg";
        selVal = $("input:checkbox[name=rdoHeadNum]:checked").map(function () {
            return this.value;
        }).get().toString();

        //$("input:checkbox[name=rdoHeadNum]:checked").each(function () {
        //    selVal = selVal + selVal.trim().length > 0 ? "," : "" + ($(this).val());
        //});
        //selVal = $("input[name='rdoHeadNum']:checked").val();
    }
    if (selVal != null) {
        switch (seltxt) {
            case "Oracle":
                genorclReport(selVal, selFlag);
                break;
            case "Excel":
                genxlReport(selVal, selFlag);
                break;
            case "SQL Server":
                break;
            default:
                alert("Please select data source");
                break;
        }
    }
    else {
        alert("Please select target columns");
        showhide('divhover', 'h');
    }
}

function genxlReport(selVal, selFlag) {
    showhide('divhover', 's');
    var sheet;
    var colName = $("#ulHeadList").find("input:checkbox[name=cheInflu]:checked").map(function () {
        return this.value;
    }).get().toString();
    colName = colName.length > 0 ? colName : "";
    if ($('#sheetSelect1 option').length < 1)
        sheet = "";
    else
        sheet = $("#sheetSelect1").select().val();

    var list = {
        sheet: sheet
        , selCol: colName
        , selAvgCol: selVal
        , fileName: $("#lblflName").text()
        , selFlag: selFlag
    };
    $.ajax({
        type: "POST",
        url: "webControlls.aspx/webgenReport",
        data: '{ "list":' + JSON.stringify(list) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.d != null && !(response.d.includes("Error"))) {
                readJson(response.d, selVal);//prntReoport(response.d);
            }
            else {
                alert(response.d);
                showhide('divhover', 'h');
            }
        },
        failure: function (response) {
            alert("Somthing went worng, Please try again.");
            showhide('divhover', 'h');
        }
    });
}

function prntReoport(ele, filename, trgVal) {
    var arr_from_json = JSON.parse(ele);
    var listHead = "";
    var listtr = "";
    listHead = listHead + "<table class='table' id='tblrpt'> <thead class='thead-light'><tr><th scope='col'>#</th>";
    var keys = Object.keys(arr_from_json);
    var subKeys = "";
    var k = 0, colcnt = 0;
    var valu = "";
    for (i in keys) {
        k++;
        subKeys = Object.keys(arr_from_json[i]);
        listtr = listtr + "<tr class='non' id='trrpt" + k + "'> <th scope='row'>" + k + "</th>";
        colcnt = 0;
        for (j in subKeys) {
            if (i == 0) {
                if (subKeys[j] == "SUPPORT" || subKeys[j] == "LIFT")
                    listHead = listHead + "<th scope='col' class='th_blue'>" + subKeys[j] + "</br><input id='txtcolsearch" + colcnt + "' class='form-control' type='text' value='' Placeholder='Search' onchange='txtcolSearch()'/></th>";
                else if (subKeys[j] == "CONDITIONAL_PROBABILITY" || subKeys[j] == trgVal || subKeys[j].includes("AVG_") || subKeys[j] == "CONFIDENCE" || subKeys[j] == "Correltion" || subKeys[j].includes("Coefficient") || subKeys[j] == "t Stat")
                    listHead = listHead + "<th scope='col' class='th_green'>" + subKeys[j] + "</br><input id='txtcolsearch" + colcnt + "' class='form-control' type='text' value='' Placeholder='Search' onchange='txtcolSearch()'/></th>";
                else
                    listHead = listHead + "<th scope='col' class='th_red'>" + subKeys[j] + "</br><input id='txtcolsearch" + colcnt + "' class='form-control' type='text' value='' Placeholder='Search' onchange='txtcolSearch()'/></th>";

            }
            if (arr_from_json[i][subKeys[j]] == null)
                valu = "";
            else
                valu = arr_from_json[i][subKeys[j]];
            listtr = listtr + "<td>" + valu + "</td>";
            colcnt = colcnt + 1;
        }
        listtr = listtr + "</tr>";
    }
    listHead = listHead + "</tr></thead><tbody>";
    listHead = listHead + listtr + "</tbody> </table >";
    $("#div_Report").empty();
    $("#div_Report").append("<label class='create__btn-create btn btn--type-03' style='margin: 1% 0; float: right;' onclick='gencsv(null);'>Download(CSV)</label>");    
    $("#div_Report").append(listHead);

    //$("#div_ReportReg").empty();
    //$("#div_ReportReg").append("<table class='table' id='tblrpt'> <thead class='thead-light'> <tr> <th scope = 'col' >#</th > <th scope='col' class='th_blue'>% Difference<br></th><th scope='col' class='th_blue'>Adj. Lift<br></th><th scope='col' class='th_red'>CO2<br></th> <th scope='col' class='th_red'>Population<br></th> <th scope='col' class='th_red'>GDP<br></th> <th scope='col' class='th_red'>Vehicle<br></th> </tr></thead> <tbody><tr class='non' id='trrpt1'> <th scope='row'>1</th> <td>0.00	</td>	<td>1.00</td>	<td>59.66</td> <td>			</td> <td>		</td> <td>		</td> </tr><tr class='non' id='trrpt2'> <th scope='row'>2</th> <td>0.68	</td>	<td>0.99</td>	<td>59.26</td> <td>	59.25	</td> <td>		</td> <td>		</td> </tr><tr class='non' id='trrpt3'> <th scope='row'>3</th> <td>-0.21	</td>	<td>1.00</td>	<td>59.78</td> <td>			</td> <td>59.78	</td> <td>		</td> </tr><tr class='non' id='trrpt4'> <th scope='row'>4</th> <td>-0.37	</td>	<td>1.00</td>	<td>59.88</td> <td>			</td> <td>		</td> <td>59.88	</td> </tr><tr class='non' id='trrpt5'> <th scope='row'>5</th>	<td>6.73	</td>	<td>0.93</td>	<td>55.77</td> <td>	58.75	</td> <td>60.78	</td> <td>		</td> </tr><tr class='non' id='trrpt6'> <th scope='row'>6</th>	<td>1.62	</td>	<td>0.98</td>	<td>58.70</td> <td>	59.70	</td> <td>		</td> <td>59.71	</td> </tr><tr class='non' id='trrpt7'> <th scope='row'>7</th> <td>0.34	</td>	<td>1.00</td>	<td>59.45</td> <td>			</td> <td>59.45	</td> <td>59.45	</td> </tr><tr class='non' id='trrpt8'> <th scope='row'>8</th> <td>7.24	</td>	<td>0.93</td>	<td>55.49</td> <td>	58.47	</td> <td>59.49	</td> <td>59.49	</td> </tr></tbody> </table>");



    $("#lblcolnum").text(colcnt);
    showhide('divhover', 'h');
    showhide("div_reff", "s");
    deleteFile(filename);
}

function cntOracle() {
    showhide('divhover', 's');
    if (
        $("#txt_Host").val().length > 0
        && $("#txt_Port").val().length > 0
        && $("#txt_Sid").val().length > 0
        && $("#txt_Table").val().length > 0
        && $("#txt_Userid").val().length > 0
        && $("#pwd_Pass").val().length > 0) {

        var list = {
            Host: $("#txt_Host").val()
            , Port: $("#txt_Port").val()
            , Sid: $("#txt_Sid").val()
            , Table: $("#txt_Table").val()
            , Userid: $("#txt_Userid").val()
            , Pass: $("#pwd_Pass").val()
        };
        $.ajax({
            type: "POST",
            url: "webControlls.aspx/webcntOracle",
            data: '{ "list":' + JSON.stringify(list) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response.d[0].key != "Error")
                    fillHead(response.d);
                else {
                    alert(response.d[0].vale);
                    showhide('divhover', 'h');
                }
            },
            failure: function (response) {
                alert(response.d[0].vale);
                showhide('divhover', 'h');
            }
        });
    }
    else {
        alert("Please fill all fields. ");
        showhide('divhover', 'h');
    }
}

function genorclReport(selVal, selFlag) {
    var colName = $("#ulHeadList").find("input:checkbox:checked").map(function () {
        return this.value;
    }).get().toString();
    colName = colName.length > 0 ? colName : "";
    if (
        $("#txt_Host").val().length > 0
        && $("#txt_Port").val().length > 0
        && $("#txt_Sid").val().length > 0
        && $("#txt_Table").val().length > 0
        && $("#txt_Userid").val().length > 0
        && $("#pwd_Pass").val().length > 0) {

        var list = {
            Host: $("#txt_Host").val()
            , Port: $("#txt_Port").val()
            , Sid: $("#txt_Sid").val()
            , Table: $("#txt_Table").val()
            , Userid: $("#txt_Userid").val()
            , Pass: $("#pwd_Pass").val()
            , selAvgCol: selVal
            , selCol: colName
            , selFlag: selFlag
        };
        $.ajax({
            type: "POST",
            url: "webControlls.aspx/webgenorclReport",
            data: '{ "list":' + JSON.stringify(list) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response.d != null && !(response.d.includes("Error"))) {
                    readJson(response.d, selVal);//prntReoport(response.d);
                }
                else {
                    alert(response.d);
                    showhide('divhover', 'h');
                }
            },
            failure: function (response) {
                alert("Somthing went worng, Please try again.");
                showhide('divhover', 'h');
            }
        });

    }
    else {
        alert("Please fill all fields. ");
        showhide('divhover', 'h');
    }
}

function readJson(ele, selVal) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.open("GET", ele, false);
    xmlhttp.send();
    if (xmlhttp.status == 200) {
        prntReoport(xmlhttp.responseText, ele, selVal);
    }
}

function deleteFile(ele) {
    var fileName = ele.split('/');
    $.ajax({
        type: "POST",
        url: "webControlls.aspx/webdeleteFile",
        data: '{fileName: "' + fileName[1] + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            //alert(response.d);
        },
        failure: function (response) {
            //alert(response);
        }
    });
}

function gencsv() {
    showhide('divhover', 's');
    var table_id ="tblrpt";
    var rows = document.querySelectorAll('table#' + table_id + ' tr');
    var csv = [];
    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll('td, th');
        for (var j = 0; j < cols.length; j++) {
            var data = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, '').replace(/(\s\s)/gm, ' ')
            data = data.replace(/"/g, '""');
            row.push('"' + data + '"');
        }
        csv.push(row.join(','));
    }
    var csv_string = csv.join('\n');
    var filename = 'export_grandreport_' + new Date().toLocaleDateString() + '.csv';
    var link = document.createElement('a');
    link.style.display = 'none';
    link.setAttribute('target', '_blank');
    link.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv_string));
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    showhide('divhover', 'h');
}

function txtcolSearch() {
    var colcnt = $("#lblcolnum").text();
    var searchtxt = "";
    var table = document.getElementById("tblrpt");
    var tr = table.getElementsByTagName("tr"); var td, txtValue;
    for (x = 1; x < tr.length; x++) {
        var myButtonClasses = document.getElementById("trrpt" + x).classList;
        if (myButtonClasses.contains("hide")) {
            myButtonClasses.remove("hide");
        }
    }
    for (var i = 0; i < colcnt; i++) {
        searchtxt = $("#txtcolsearch" + i).val();
        if (searchtxt != null && searchtxt.trim().length > 0)
            for (j = 1; j < tr.length; j++) {
                var myButtonClasses = document.getElementById("trrpt" + j).classList;
                if (myButtonClasses.contains("hide")) {
                }
                else {
                    td = tr[j].getElementsByTagName("td")[i];
                    if (td) {
                        txtValue = td.textContent || td.innerText;
                        if ((txtValue.toUpperCase()).indexOf(searchtxt.toUpperCase()) > -1) {

                        } else {
                            document.getElementById("trrpt" + j).className += " hide ";
                        }
                    }
                }
            }
    }
}

function cntSql() {
    showhide('divhover', 's');
    if (
        $("#txt_Source").val().length > 0
        && $("#txt_db").val().length > 0
        && $("#txt_SUserid").val().length > 0
        && $("#pwd_SPass").val().length > 0) {

        var list = {
            Host: $("#txt_Source").val()
            , Port: $("#txt_db").val()
            , Userid: $("#txt_SUserid").val()
            , Pass: $("#pwd_SPass").val()
        };
        $.ajax({
            type: "POST",
            url: "webControlls.aspx/webcntSql",
            data: '{ "list":' + JSON.stringify(list) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response.d[0].key != "Error") {
                    if (response.d[0].key == "Sheet")
                        fillSheet(response.d);
                    else
                        filltable(response.d);
                }
                else {
                    alert(response.d[0].vale);
                    showhide('divhover', 'h');
                }
            },
            failure: function (response) {
                alert(response.d[0].vale);
                showhide('divhover', 'h');
            }
        });
    }
    else {
        alert("Please fill all fields. ");
        showhide('divhover', 'h');
    }
}

function filltable(ele) {
    var listtr = "<label class='create__btn-create btn btn--type-03' style='margin: 1% 0;' onclick='selectAll();'>Move Columns</label> <div class='topic__list'><ul id=ulColumnList class='list'>  <li> <span> <input id='chkColAll' type='checkbox' onchange='chkChangeColall();' value='All'></span>  <span>Select All</span></li>";
    var chid = 0;
    for (i = 0; i < ele.length; i++) {
        var dtType = ele[i].DType.split(".");
        if ((numDtType.includes(dtType[1])) == true || (numDtType.includes(dtType)) == true) {
            chid++;
            listtr = listtr + " <li> <span><input id='chkCol" + chid + "' type='checkbox'  value='" + ele[i].vale + "'></span>  <span>" + ele[i].vale + "</span></li>";
        }
    }
    listtr = listtr + " </ul></div> ";
    $("#div_Headlist").empty();
    $("#div_Headlist").append(listtr);
    //$('#tblrpt').DataTable();
    showhide("div_Headlist", "s");
    showhide('divhover', 'h');
}

function chkChangeColall() {
    var colName = $("#ulColumnList").find("input:checkbox").map(function () {
        return this.value;
    }).get().toString().split(',');
    for (i = 0; i < colName.length; i++) {
        var k = i - 1;
        if ($("#chkColAll").prop("checked"))
            $("#chkCol" + i).prop("checked", true);
        else
            $("#chkCol" + i).prop("checked", false);
    }
}

function getColumns(ele) {
    showhide('divhover', 's');

    var list = {
        Host: $("#txt_Source").val()
        , Port: $("#txt_db").val()
        , Userid: $("#txt_SUserid").val()
        , Pass: $("#pwd_SPass").val()
        , Table: $("#" + ele).select().val()//$("#tbl_" + ele).html()
    };
    $.ajax({
        type: "POST",
        url: "webControlls.aspx/webcntSqlTable",
        data: '{ "list":' + JSON.stringify(list) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.d[0].key != "Error") {
                filltable(response.d);
            }
            else {
                alert(response.d[0].vale);
                showhide('divhover', 'h');
            }
        },
        failure: function (response) {
            alert(response.d[0].vale);
            showhide('divhover', 'h');
        }
    });
}

function selectAll() {
    showhide('divhover', 's');
    var colName = $("#ulColumnList").find("input:checkbox:checked").map(function () {
        return this.value;
    }).get().toString().split(',');
    var tblName = $("#sheetSelect1").select().val();
    var rowCount = parseInt($("#lblhCount").html());
    if (colName[0].length > 0) {
        for (i = 0; i < colName.length; i++) {
            if ($('#tblrpt tr > td:contains(' + tblName + ') + td:contains(' + colName[i] + ')').length < 1) {
                if (colName[i] != "All") {
                    $("#chkTrgAll").prop("checked", false);
                    $("#chkCovAll").prop("checked", false);
                    var listtr = listtr + "<tr class='non' id='trrpt" + (rowCount + 1) + "'>"
                        + "<td><input id='chkTarget" + rowCount + "' type='checkbox' name='chkTarget' onchange='chkChange(" + rowCount + ",1);' value='" + colName[i] + "'></td>"
                        + "<td><input id='chkCovar" + rowCount + "' type='checkbox' name='chkCovar' onchange='chkChange(" + rowCount + ",2);' value='" + colName[i] + "'></td>"
                        + "<td id='tdTbl_" + rowCount + "'>" + tblName + "</td>"
                        + "<td id='tdCol_" + rowCount + "'>" + colName[i] + "</td>"
                        + "<td class='tdtext'><input id='txt_" + rowCount + "' type='text' class='txtWhere' onblur='chkChange(" + rowCount + ",3)' /></td>"
                        + "<td><input id='chkGrpBy" + rowCount + "' type='checkbox' name='chkGrpBy' onchange='chkChange(" + rowCount + ",4);' value='" + colName[i] + "'></td></tr>";
                    rowCount++;
                }
            }
        }
        $("#lblhCount").html(rowCount);
        //$("#tbl_column").empty();
        $("#tbl_column").append(listtr);
        showhide("div_Columnlist", "s");
    }
    showhide('divhover', 'h');
}

function chkChangetblall(eleid, elenxtid, ele, elenxt) {
    if ($("#" + eleid).prop("checked")) {
        $('input[name=' + ele + ']').attr('checked', true);
        $('input[name=' + elenxt + ']').attr('checked', false);
        $("#" + elenxtid).prop("checked", false);
    }
    else
        $('input[name=' + ele + ']').attr('checked', false);
}

function genCorrel() {
    showhide('divhover', 's');
    var listTarget = [];
    var listCovar = [];
    var listGrpBy = [];
    var listWhere = [];

    var rows = $('#tbl_column >tr');
    for (var i = 1; i < rows.length; i++) {
        if ($("#chkTarget" + i).prop("checked")) {
            var chkTarget = {
                tblName: $("#tdTbl_" + i).html()
                , colName: $("#tdCol_" + i).html()
            };
            listTarget.push(chkTarget);
        }
        if ($("#chkCovar" + i).prop("checked")) {
            var chkCovar = {
                tblName: $("#tdTbl_" + i).html()
                , colName: $("#tdCol_" + i).html()
            };
            listCovar.push(chkCovar);
        }
        if ($("#chkGrpBy" + i).prop("checked")) {
            var chkGrpBy = {
                tblName: $("#tdTbl_" + i).html()
                , colName: $("#tdCol_" + i).html()
            };
            listGrpBy.push(chkGrpBy);
        }
        if ($("#txt_" + i).val().length > 0) {
            var txtWhere = {
                tblName: $("#tdTbl_" + i).html()
                , colName: $("#tdCol_" + i).html()
                , wherCond: $("#txt_" + i).val()
            };
            listWhere.push(txtWhere);
        }
    }

    var list = {
        Target: JSON.stringify(listTarget)
        , Covariance: JSON.stringify(listCovar)
        , WhereCon: JSON.stringify(listWhere)
        , GrpBy: JSON.stringify(listGrpBy)
    };
    var dblist = {
        Host: $("#txt_Source").val()
        , Port: $("#txt_db").val()
        , Userid: $("#txt_SUserid").val()
        , Pass: $("#pwd_SPass").val()
    };

    $.ajax({
        type: "POST",
        url: "webControlls.aspx/webSqlCorrelation",
        data: '{ "list":' + JSON.stringify(list) + ',"dblist":' + JSON.stringify(dblist) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.d != null && !response.d[0].xVar != "Error") {
                printCorrelation(response.d);
            }
            else {
                alert(response.d[0].yVar);
                showhide('divhover', 'h');
            }
        },
        failure: function (response) {
            alert("Somthing went worng, Please try again.");
            showhide('divhover', 'h');
        }
    });

}

function printCorrelation(ele) {
    var listtr = "";
    $("#tbdyCorr").empty();
    $("#lblxvar").html("X-Part : " + ele[0].xVar);
    for (i = 0; i < ele.length; i++) {
        listtr = listtr + "<tr><td class='colleft'>" + ele[i].yVar + "</td ><td class='colright'>" + ele[i].corValue + "</td></tr >";
    }
    $("#tbdyCorr").append(listtr);
    showhide('popup1', 's');
    $('#tblrptCor').DataTable();
    showhide('divhover', 'h');
}

function closePop() {
    showhide('popup1', 'h');
    $("#tbdyCorr").empty();
}




//{
//    var choice = {};
//    choice.url = "Handler2.ashx";
//    choice.type = "POST";
//    choice.data = fromdata;
//    choice.dataType = json;
//    choice.contentType = application/json; charset=utf-8;
//    choice.success = function (result) { alert(result); };
//    choice.error = function (err) { alert(err.statusText); };
//    $.ajax(choice);
//    event.preventDefault();}


//var dtType = ele[i].DType.split(".");
//if ((numDtType.includes(dtType[1])) == true || (numDtType.includes(dtType)) == true)
//    listtr = listtr + " <li> <span><input type='radio' id='rdoAvg" + i + "' name='rdoHeadNum' value='" + ele[i].vale + "' onchange='rdoAvgchange(" + i + ");'> </span>  <span><input id='chkCol" + i + "' type='checkbox' onchange='chkChange(" + i + ");' value='" + ele[i].vale + "'></span>  <span>" + ele[i].vale + "</span></li>";
//else
//    listtr = listtr + " <li> <span><input type='radio' id='rdoAvg" + i + "' name='rdoHeadDis' value='" + ele[i].vale + "' onchange='rdoDicchange(" + i + ");'> </span>  <span><input id='chkCol" + i + "' type='checkbox' onchange='chkChange(" + i + ");' value='" + ele[i].vale + "'></span>  <span>" + ele[i].vale + "</span></li>";


 //$("input:checkbox[name=rdoHeadNum]:checked").each(function () {
    //    alert($(this).val());
    //});