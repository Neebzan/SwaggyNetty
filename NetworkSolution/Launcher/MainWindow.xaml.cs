﻿using GlobalVariablesLib.Models;
using PatchManagerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public Frame mainFrame;
        public Button playButton;
        private ProgressBar progressBar;

        public MainWindow () {

            InitializeComponent();
            mainFrame = frame;
            playButton = play_button;
            playButton.IsEnabled = false;
            playButton.Opacity = .5;
            progressBar = progress_bar;
            Backend.InitiatePatchClient();
            //progressBar.Value = Backend.PatchProgress;
            //patchpercentage_label.Content = Backend.PatchProgress.ToString() + "%";

            mainFrame.NavigationService.Navigate(new LoginPage());


            PatchmanagerClient.MissingFilesUpdated += (object sender, EventArgs e) => {
                patchpercentage_label.Dispatcher.Invoke(() => {
                    patchpercentage_label.Content = String.Format("{0:0.##}", Backend.PatchProgress) + "%";
                    progressBar.Value = Backend.PatchProgress;
                });
            };
        }
    }
}
