﻿using BoldReports.RDL.DOM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string filePath = Environment.CurrentDirectory;
            filePath = filePath.Replace("bin\\Debug\\net7.0-windows", "");
            string reportPath = filePath + @"Resources\barcode.rdl";
            this.reportViewer.ReportPath = reportPath;
            this.reportViewer.RefreshReport();
        }
    }
}
