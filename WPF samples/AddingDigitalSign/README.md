# How to add digital signature in PDF export in WPF application

## Getting Started

rRefer to the [Getting Started](https://help.boldreports.com/embedded-reporting/wpf-reporting/report-viewer/display-ssrs-rdl-report-in-wpf-application/) documentation to set up the WPF viewer application.

## Invoking the Report Export Event in the Application

In the report export method, utilize the report export event to pass the report path to the report writer class. Then, save it as a PDF stream.

## Adding Digital Signature in WPF

Now, pass the stream into the PdfLoadedDocument property and initiate the customization process to add a digital signature. Provide the signature image and include it in the exported PDF report document by specifying the graphics for it.

Next, save the PDF with the added signature image as a stream and convert it to a byte array. Specify the location to save the PDF file.

## Support and Feedback

* For any other queries, reach out to our [Bold Reports support team](mailto:support@boldreports.com)