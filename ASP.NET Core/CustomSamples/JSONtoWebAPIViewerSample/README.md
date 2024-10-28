# How to Change RDL datasource from JSON to WebAPI in ASP.NET Core sample Bold Reports

This project was created using ASP.NET Core 8.0. This application aims to demonstrate how to change RDL datasource from JSON to WebAPI in ASP.NET Core sample Bold Reports.

## Requirements to run the sample

The samples requires the below requirements to run.

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) and above
* .Net Framework 8.0 and above

## Using the Reporting Samples

* Open the solution file `JSONtoWebAPIViewerSample.sln` in Visual Studio.

* The following Reports dependencies will be installed automatically at compile time.

Package | Purpose
--- | ---
`BoldReports.Net.Core` | Creates Web API service is used to process the reports.
`BoldReports.AspNet.Core` | Contains tag helpers to create client-side reporting control

* Press `F5` or click the `Run` button in Visual Studio to launch the application.

## Why this sample?

This sample demonstrates how to dynamically change the data source of a report from JSON to a WebAPI endpoint at runtime in Bold Reports. This is particularly useful for fetching real-time data from a WebAPI endpoint instead of relying on static JSON data. The process involves modifying the report definition file (RDL) to update its data source, allowing for more dynamic and up-to-date reporting.

## Documentation

A complete Bold Reports documentation for using Report Viewer control in ASP.NET Core sample application can be found on [ASP.NET Core](https://help.boldreports.com/embedded-reporting/aspnet-core-reporting/report-viewer/display-ssrs-rdl-report-in-asp-net-core-application/).

## Support

* If you encounter any challenges or have inquiries regarding report creation or the issues in the reports, please do not hesitate to open a [support ticket](https://support.boldreports.com/support) with us. This allows us to investigate the matter and offer assistance to resolve any issues.
