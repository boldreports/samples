# Integrating Barcode Extension with Bold Reports in WPF .NET Core

Enhance your WPF .NET Core applications' reporting capabilities by integrating barcode functionality using Bold Reports. Below are the steps to add a barcode extension to the Bold Reports Report Viewer in a WPF .NET Core application.

## Requirements

Ensure you have the following prerequisites before proceeding:

* Visual Studio 2019 or later
* .NET 5.0 or later

## Running the Sample

1. Open the solution file `ReportViewerWPFCore.sln` in Visual Studio.
2. The necessary dependencies will be installed automatically upon compilation:

   | Package                   | Purpose                                                      |
   | ------------------------- | ------------------------------------------------------------ |
   | `BoldReports.WPF`| Contains WPF Reporting controls (Report Viewer and Report Writer) to preview and export the reports.       |

3. Press `F5` or click the `Run` button in Visual Studio to launch the application.

## Getting Started

Refer to the [Getting Started](https://help.boldreports.com/embedded-reporting/wpf-reporting/report-viewer/display-ssrs-rdl-report-in-wpf-net-core-application/) documentation to set up the WPF .NET Core Report viewer application.

## Integration Steps

Follow these steps to integrate barcode functionality with Bold Reports in your WPF .NET Core application:

1. **Create or Open Your WPF .NET Core Project:**
   Open your existing WPF .NET Core project in Visual Studio or create a new one.

2. **Install Bold Reports NuGet Package:**
   Install the Bold Reports WPF NuGet package by adding the following package reference to your project file:
  <PackageReference Include="BoldReports.Net.WPF" Version="latest" />
   ```
3. Add Barcode Extension to the Report Viewer:
Update your XAML markup for the Bold Reports Report Viewer to include the barcode extension:
<BoldReports.WPF.ReportViewer x:Name="reportViewer" BarcodeExtension="True" />

4. Design and Generate Reports with Barcode:
Design your reports using the Bold Reports Report Designer and include barcode elements as needed.

5. Run and Test Your Application:
Build and run your WPF .NET Core application to view the reports with barcode functionality integrated.

## Learn More
For detailed documentation and tutorials on integrating barcode extensions with Bold Reports in WPF .NET Core applications, refer to the Bold Reports Documentation.

## Support
If you encounter any challenges or have inquiries regarding report creation or issues in the reports, please do not hesitate to open a [support ticket](https://support.boldreports.com/support) with us. This allows us to investigate the matter and offer assistance to resolve any issues.