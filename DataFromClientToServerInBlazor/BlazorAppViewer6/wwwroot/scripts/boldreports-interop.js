﻿// Interop file to render the Bold Report Viewer component with properties.
window.BoldReports = {
    RenderViewer: function (elementID, reportViewerOptions) {
        $("#" + elementID).boldReportViewer({
            reportPath: reportViewerOptions.reportName,
            reportServiceUrl: reportViewerOptions.serviceURL,
            ajaxBeforeLoad: function (args) {
                //Passing custom data to server
                args.data = reportViewerOptions.dataBaseData;
            }
        });
    }
}
