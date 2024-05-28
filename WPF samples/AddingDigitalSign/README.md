# How to add digital signature in PDF export in WPF application

## Getting Started

Refer to the [Getting Started](https://help.boldreports.com/embedded-reporting/wpf-reporting/report-viewer/display-ssrs-rdl-report-in-wpf-application/) documentation to set up the WPF viewer application.

## Invoking the Report Export Event in the Application

In the report export method, utilize the [report viewer export event](https://help.boldreports.com/embedded-reporting/cr/wpf-reporting/BoldReports.Windows.ExportEventHandler.html) to pass the report path to the report writer class. Then, save it as a PDF stream.

## Adding Digital Signature in WPF

Now, pass the stream into the [PdfLoadedDocument](https://help.syncfusion.com/cr/file-formats/Syncfusion.Pdf.Parsing.PdfLoadedDocument.html) class by referring to the [PDF documentation](https://help.syncfusion.com/file-formats/pdf/working-with-digitalsignature#adding-a-digital-signature-using-x509certificate2) and initiate the customization process to add a digital signature. Follow the guidelines provided in the documentation to integrate digital signature functionality seamlessly. Utilize the [PDFSignature](https://help.syncfusion.com/cr/file-formats/Syncfusion.Pdf.Security.PdfSignature.html) class to provide the signature image and include it in the exported PDF report document by specifying the graphics for it. 

After adding the digital signature, save the PDF with the added signature image as a stream and convert it to a byte array. Specify the location to save the PDF file according to your application's requirements.

## Support and Feedback

* For any other queries, reach out to our [Bold Reports support team](mailto:support@boldreports.com)