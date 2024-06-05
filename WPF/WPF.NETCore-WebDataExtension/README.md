# Implementing JSON Data Source with Bold Reports in WPF .NET Core

Elevate your WPF .NET Core applications' reporting capabilities by integrating JSON data source functionality with Bold Reports. Follow these instructions to incorporate a JSON data source into the Bold Reports Report Viewer within your WPF .NET Core application.

## Prerequisites

Before you begin, ensure you have the following:

* Visual Studio 2019 or newer
* .NET 5.0 or higher

## Running the Example

1. In Visual Studio, open the solution file `ReportViewerWPFCore.sln`.
2. Dependencies will be automatically installed when you build the project:

   | Package              | Purpose                                                      |
   | -------------------- | ------------------------------------------------------------ |
   | `BoldReports.WPF` | Includes WPF Reporting controls (Report Viewer and Report Writer) for report preview and exportation. |

3. To start the application, press `F5` or click the `Run` button in Visual Studio.

## Initial Setup

Consult the [Getting Started](https://help.boldreports.com/embedded-reporting/wpf-reporting/report-viewer/display-ssrs-rdl-report-in-wpf-net-core-application/) guide for instructions on setting up the Report Viewer application for WPF .NET Core.

## Integration Procedure

To integrate JSON data source functionality with Bold Reports in your WPF .NET Core application, perform the following steps:

1. **Open or Create Your WPF .NET Core Project:**
   Launch your existing WPF .NET Core project in Visual Studio or start a new project.

2. **Install Bold Reports NuGet Package:**
   Add the Bold Reports WPF NuGet package to your project by including the following package reference in your project file:
   ```xml
   <PackageReference Include="BoldReports.WPF" Version="latest" />
   ```

3. **Incorporate JSON Data Source in Report Viewer:**
   Update the XML configuration in the `App.config` file to enable the Report Viewer from Bold Reports to utilize a JSON data source:
   ```xml
   <DataExtension>
      <Extension Name="JSON" Assembly="BoldReports.Data.WebData" Type="BoldReports.Data.WebData.JSONExtension" />
    </DataExtension>
   ```

4. **Design and Generate Reports with JSON Data Source:**
   Utilize the Bold Reports Report Designer to create your reports, ensuring to integrate JSON data sources as required.

5. **Execute and Validate Your Application:**
   Compile and execute your WPF .NET Core application to ensure that the reports are correctly displaying data from the JSON data source.

## Additional Information

For comprehensive guides and tutorials on adding JSON data sources to Bold Reports in WPF .NET Core applications, please refer to the [Bold Reports Documentation](https://help.boldreports.com/).

## Support

Should you encounter any issues or have questions about report creation or specific report problems, feel free to open a [support ticket](https://support.boldreports.com/support) with us. We'll be happy to investigate and assist you in resolving any concerns.