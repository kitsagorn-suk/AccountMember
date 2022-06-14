$(document).ready(function () {
    $("#btnLoadReport").cilck(function () {
        ReportManager.LoadReport();
    });
});

var ReportManager= {
    LoadReport: function () {
        var jsonParam = "";
        var serviceUrl = "../Quotation/GenerateRpt";
        ReportManager.GetReport(serviceUrl, jsonParam, onFailed);
        function onFailed(error) {
            alert("Found Error");
        }
    },
    GetReport: function (serviceUrl, jsonParams, errorCallback) {
        jQuery.ajax({
            url: serviceUrl,
            async: false,
            type: "POST",
            data: "{" + jsonParams + "}",
            contentType: "application/json; charset=utf-8",
            success: function () {
                window.open('../Report/ReportViewer.aspx', '_newtab');
            },
            error: errorCallback
        });
    }
}