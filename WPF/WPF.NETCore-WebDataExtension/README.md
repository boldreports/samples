# Integrating JSON, XML, WEBAPI, and OData Data Sources in WPF Report Viewer

Integrating various data sources such as [JSON](https://help.boldreports.com/standalone-report-designer/designer-guide/manage-data/data-connectors/json-data-source/), [XML](https://help.boldreports.com/standalone-report-designer/designer-guide/manage-data/data-connectors/xml-data-source/), [WEBAPI](https://help.boldreports.com/standalone-report-designer/designer-guide/manage-data/data-connectors/rest-api-data-source/), and [OData](https://help.boldreports.com/standalone-report-designer/designer-guide/manage-data/data-connectors/odata-data-source/) into your [WPF .NET Core Report Viewer](https://help.boldreports.com/embedded-reporting/wpf-reporting/report-viewer/display-ssrs-rdl-report-in-wpf-net-core-application/)  application can enhance its data processing capabilities. This article provides a step-by-step guide on adding the JSON data extension to the WPF Bold Reports Viewer sample application, with additional information on integrating other data sources.

## Prerequisites

Ensure you have the following prerequisites before proceeding:

* Visual Studio 2019 or later
* .NET 5.0 or higher

## Running the Example

1. In Visual Studio, open the solution file **ReportViewerWPFCore.sln**.
2. Dependencies will be automatically installed when you build the project:

   | Package              | Purpose                                                      |
   | -------------------- | ------------------------------------------------------------ |
   | `BoldReports.WPF` | Includes WPF Reporting controls (Report Viewer and Report Writer) for report preview and exportation. |

3. To start the application, press **F5** or click the **Run** button in Visual Studio.

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
3. **Add Project Reference**
   Add the custom WebData Extension **BoldReports.Data.WebData_Core** project reference to your solution. Refer to the image below for guidance.
   ![JSONExtension.png](https://support.boldreports.com/kb/agent/attachment/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIzNzM4Iiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5ib2xkcmVwb3J0cy5jb20ifQ.uC0pGnq9liRqUduCLbVe4TojcsVsFiW79N4kzdgetSY)

4. **Incorporate JSON Data Source in Report Viewer:**
   Update the XML configuration in the **App.config** file to enable the Report Viewer from Bold Reports to utilize a JSON data source:

    ```xml
    <configuration>
      <configSections>
        <section name="ReportingExtensions" type="BoldReports.Configuration.Extensions,  BoldReports.WPF" allowLocation="true" allowDefinition="Everywhere" />
      </configSections>
      <ReportingExtensions>
        <DataExtension>
          <Extension Name="JSON" Assembly="BoldReports.Data.WebData" Type="BoldReports.Data.WebData.JSONExtension" />
          <Extension Name="OData" Assembly="BoldReports.Data.WebData" Type="BoldReports.Data.WebData.ODataExtension" />
          <Extension Name="WebAPI" Assembly="BoldReports.Data.WebData" Type="BoldReports.Data.WebData.WebAPIExtension" />
          <Extension Name="XML" Assembly="BoldReports.Data.WebData" Type="BoldReports.Data.WebData.XMLExtension" />
        </DataExtension>
      </ReportingExtensions>
    </configuration>
    ```

**Note**: To integrate **XML**, **WEBAPI**, or **OData** data sources, add the corresponding extension in the **App.config** file as shown above.

5. **Design and Generate Reports with JSON Data Source:**
   Utilize the Bold Reports Report Designer to create your reports, ensuring to integrate JSON data sources as required.

6. **Execute and Validate Your Application:**
   Compile and execute your WPF .NET Core application to ensure that the reports are correctly displaying data from the JSON data source.
   ![Output.png](https://support.boldreports.com/kb/agent/attachment/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIzNzM5Iiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5ib2xkcmVwb3J0cy5jb20ifQ.jSQd8DW01qHI9nWO5Qbd7QfRPG8rOAHwdUpU5WJGri8)

## Additional Information

For comprehensive guides and tutorials on adding JSON data sources to Bold Reports in WPF .NET Core applications, please refer to the [Bold Reports Documentation](https://help.boldreports.com/).

## Support

Should you encounter any issues or have questions about report creation or specific report problems, feel free to open a [support ticket](https://support.boldreports.com/support) with us. We'll be happy to investigate and assist you in resolving any concerns.