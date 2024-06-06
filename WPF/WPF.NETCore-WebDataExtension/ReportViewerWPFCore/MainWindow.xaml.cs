using BoldReports.Data.WebData;
using BoldReports.RDL.DOM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
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

namespace ReportViewerWPFCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string currentDirectory = Environment.CurrentDirectory;
            int binIndex = currentDirectory.IndexOf("bin", StringComparison.OrdinalIgnoreCase);
            string filePathWithoutBin = binIndex >= 0 ? currentDirectory.Substring(0, binIndex) : currentDirectory;
            string reportPath = filePathWithoutBin + @"Resources\JSONReport.rdl";
            this.reportViewer.ReportPath = reportPath;
            this.reportViewer.RefreshReport();
        }
    }
}
