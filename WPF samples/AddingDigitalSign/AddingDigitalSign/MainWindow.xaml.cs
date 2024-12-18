﻿using BoldReports.Writer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace AddingDigitalSign
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)

        {

            this.reportViewer.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Resource\sales-order-detail.rdl");
            this.reportViewer.ReportExport += ReportViewer_ReportExport;
            this.reportViewer.RefreshReport();
        }

        private void ReportViewer_ReportExport(object sender, BoldReports.Windows.ReportExportEventArgs e)
        {
            //Get report path
            string reportPath = this.reportViewer.ReportPath;

            // Set up report writer
            ReportWriter reportWriter = new ReportWriter(reportPath);
            reportWriter.ReportProcessingMode = ProcessingMode.Remote;

            // Create a MemoryStream to store the generated PDF report
            MemoryStream stream = new MemoryStream();
            // Save the report generated by the report writer into the MemoryStream in PDF format
            reportWriter.Save(stream, WriterFormat.PDF);

            //Load the PDF document
            PdfLoadedDocument document = new PdfLoadedDocument(stream);

            //Gets the page  
            PdfLoadedPage page = document.Pages[0] as PdfLoadedPage;

            //Create PDF graphics for the page
            PdfGraphics graphics = page.Graphics;

            // Replace "bin\Debug" with an empty string
            string projectDirectory = Environment.CurrentDirectory.Replace(@"\bin\Debug", "");

            // Create relative paths for the certificate and signature image
            string certificatePath = System.IO.Path.Combine(projectDirectory, @"Resource\PDF\PDF.pfx");
            string signatureImagePath = System.IO.Path.Combine(projectDirectory, @"Resource\PDF\signature.jpg");

            // Creates a certificate instance from PFX file with private key
            X509Certificate2 certificate = new X509Certificate2(certificatePath, "password123");
            PdfCertificate pdfCertificate = new PdfCertificate(certificate);

            // Creates a signature field
            PdfSignature signature = new PdfSignature(document, page, pdfCertificate, "Signature");
            // Sets an image for the signature field
            FileStream imageStream = new FileStream(signatureImagePath, FileMode.Open, FileAccess.Read);
            PdfBitmap signatureImage = new PdfBitmap(imageStream);

            //Draws the signature image
            graphics.DrawImage(signatureImage, 50, 480, 70, 30);

            //Creating the stream object
            MemoryStream stream1 = new MemoryStream();
            //Save the document as stream
            document.Save(stream1);
            //If the position is not set to '0', then the PDF will be empty
            stream1.Position = 0;
            //Close the document
            document.Close(true);
            //Defining the ContentType for pdf file
            string contentType = "application/pdf";

            // Convert the stream to byte array
            byte[] byteArray = stream1.ToArray();
            // Specify the file path for saving the PDF
            string pdffilepath = @"D:\";
            // Write the byte array to a PDF file
            File.WriteAllBytes(pdffilepath + e.FileName + ".pdf", byteArray);

            // Indicate cancellation of default export behavior
            e.Cancel = true;

            // Show export confirmation message
            MessageBox.Show("Report Exported");
        }
    }
}
