using GlobalVariablesLib.Models;
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
using System.Windows.Threading;

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
            Backend.BackendErrorEncountered += HandleBackendError;
            mainFrame.NavigationService.Navigate(new LoginPage());



            Application.Current.MainWindow.Deactivated += (object sender, EventArgs e) => {
                Error_Popup.IsOpen = false;
            };

            Application.Current.MainWindow.LocationChanged += (object sender, EventArgs e) => {
                Error_Popup.IsOpen = false;
            };
            this.KeyDown += (object sender, KeyEventArgs e) => {
                Error_Popup.IsOpen = false;
            };

            Error_Popup.IsOpen = false;

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
                            patch_status_label.Content = "Client is up to date";
                            break;
                        default:
                            break;
                    }
                });
            };
        }

        private void HandleBackendError (object sender, BackendErrorEventArgs e) {
            DisplayError(e.ErrorTitle, e.ErrorMessage);
        }

        public void DisplayError (string _errTitle, string _errMsg) {
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => {
                Error_Popup_Label.Text = _errTitle;
                Error_Popup_Label.Text += "\n\n" + _errMsg;
                Error_Popup.IsOpen = true;
            }));
        }

        private void StartBackend (object sender, RoutedEventArgs e) {
            Backend.InitiatePatchClient();
        }

        private void Play_button_Click (object sender, RoutedEventArgs e) {
            Backend.LaunchGame();
        }
    }
}
