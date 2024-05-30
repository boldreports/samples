# Dynamically Changing the XML DataSource in Bold Reports at Runtime in V2.0 Report Viewer in Blazor Sample

This project was created using the Blazor .NET 7.0 Framework. It demonstrates dynamically changing the XML DataSource in Bold Reports at runtime using the V2.0 Report Viewer in a Blazor application.

## Requirements

To run the sample, make sure you have the following prerequisites:

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) or later
* .NET 6.0 or later

## Running the Sample

1. Open the solution file `ReportViewerBlazor.sln` in Visual Studio.
2. The necessary dependencies will be installed automatically upon compilation:

   | Package                   | Purpose                                                      |
   | ------------------------- | ------------------------------------------------------------ |
   | `BoldReports.Net.Core`    | Creates a Web API service used to process the reports.       |
   | `BoldReports.Data.WebData` and `BoldReports.Data.Csv`  | Provides XML data source extensions.|

3. Press `F5` or click the `Run` button in Visual Studio to launch the application.

## Getting Started

To set up the Blazor Report Viewer application, refer to the [Getting Started](https://help.boldreports.com/embedded-reporting/blazor-reporting/report-viewer/add-report-viewer-to-a-blazor-application/) documentation.

The new Report Viewer v2.0 component has been introduced. Refer to the following documentation for migration guides: [Migrate to Report Viewer v2.0 in Blazor](https://help.boldreports.com/embedded-reporting/blazor-reporting/report-viewer/how-to/migrate-report-viewer-v2/).

## Programmatically Changing the XML DataSource at Runtime in Blazor

In this sample application, you can dynamically change the XML data source at runtime and render the reports. This can be achieved by deserializing the XML data (read as a text file) and loading it into the report at runtime in the `OnInitReportOptions` method.

## Support

If you encounter any challenges or have inquiries regarding report creation or issues in the reports, please do not hesitate to open a [support ticket](https://support.boldreports.com/support) with us. This allows us to investigate the matter and offer assistance to resolve any issues.