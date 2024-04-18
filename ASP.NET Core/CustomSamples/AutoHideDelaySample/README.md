# How to AutoHide the Floating toolbar in V2.0 Report Viewer in ASP.NET Core sample Bold Reports

This project was created using ASP.NET Core 6.0. This application aims to demonstrate how to auto hide the floating toolbar in V2.0 Report Viewer.

## Requirements to run the sample

The samples requires the below requirements to run.

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) and above
* .Net Framework 6.0 and above

## Using the Reporting Samples

* Open the solution file `AutoHideDelaySample.sln` in Visual Studio.

* The following Reports dependencies will be installed automatically at compile time.

Package | Purpose
--- | ---
`BoldReports.Net.Core` | Creates Web API service is used to process the reports.
`BoldReports.AspNet.Core` | Contains tag helpers to create client-side reporting control

* Press `F5` or click the `Run` button in Visual Studio to launch the application.

## Why this sample?

The [Bold Reports V2 report viewer offers a feature](https://help.boldreports.com/embedded-reporting/aspnet-core-reporting/report-viewer/how-to/migrate-report-viewer-v2/) to automatically hide the floating report toolbar after a specific duration, enhancing user experience when scrolling through data-rich first pages. This functionality is achieved using the `AutoHideDelay` property within the toolbar settings. By default, the toolbar disappears after 5 seconds, but users can customize this duration as needed. It avoids the scenario where the toolbar covers essential report content, providing unobstructed access to data.

## Documentation

A complete Bold Reports documentation for using Report Viewer control in ASP.NET Core sample application can be found on [ASP.NET Core](https://help.boldreports.com/embedded-reporting/aspnet-core-reporting/report-viewer/display-ssrs-rdl-report-in-asp-net-core-application/).

## Support and Feedback

* For any other queries, reach our [Bold Reports support team](mailto:support@boldreports.com) or [Feedback portal](https://www.boldreports.com/feedback/).

* To renew the subscription, click [here](https://www.boldreports.com/pricing/on-premise) or contact our sales team at <https://www.boldreports.com/contact>.
