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

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Frame mainFrame;
        public Button playButton;
        private BaseProgressBar progressBar;

        public MainWindow () {

            InitializeComponent();
            mainFrame = frame;
            playButton = play_button;
            playButton.IsEnabled = false;
            playButton.Opacity = .3;
            progressBar = progress_bar;

            Loaded += StartBackend;
            mainFrame.NavigationService.Navigate(new LoginPage());


            PatchmanagerClient.MissingFilesUpdated += (object sender, EventArgs e) => {
                patchpercentage_label.Dispatcher.Invoke(() => {
                    patchpercentage_label.Content = String.Format("{0:0.##}", Backend.PatchProgress) + "%";
                    //progressBar.Value = Backend.PatchProgress;
                    progressBar.NewValueGiven?.Invoke(this, new ProgressBarValueChangedEventArgs((float)progressBar.Value, Backend.PatchProgress));
                    if (PatchmanagerClient.MissingFiles.Files.Count > 0) {
                        files_remaining_label.Content = "Files remaining: " + PatchmanagerClient.MissingFiles.Files.Count.ToString();
                        file_label.Content = "File: " + PatchmanagerClient.MissingFiles.Files [ 0 ].FilePath;
                    }
                });
            };

            PatchmanagerClient.DownloadComplete += (object sender, EventArgs e) => {
                patchpercentage_label.Dispatcher.Invoke(() => {
                    patchpercentage_label.Content = String.Format("{0:0.##}", Backend.PatchProgress) + "%";
                    progressBar.NewValueGiven?.Invoke(this, new ProgressBarValueChangedEventArgs((float)progressBar.Value, Backend.PatchProgress));
                    files_remaining_label.Content = "";
                    file_label.Content = "";
                });
            };

            PatchmanagerClient.StatusChanged += (object sender, EventArgs e) => {
                patchpercentage_label.Dispatcher.Invoke(() => {
                    switch (PatchmanagerClient.Status) {
                        case PatchStatus.Connecting:
                            patch_status_label.Content = "Connecting to patching server..";
                            break;
                        case PatchStatus.Downloading:
                            patch_status_label.Content = "Downloading..";
                            break;
                        case PatchStatus.Done:
                            patch_status_label.Content = "Download completed";
                            break;
                        default:
                            break;
                    }
                });
            };
        }

        private void StartBackend (object sender, RoutedEventArgs e) {
            Backend.InitiatePatchClient();
        }
    }
}
