using Launcher.Properties;
using PatchManagerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for LoggedInPage.xaml
    /// </summary>
    public partial class LoggedInPage : BasePage {
        Label loggedInAs;

        public LoggedInPage () {
            InitializeComponent();

            loggedInAs = logged_in_as_label;
            loggedInAs.Content += " " + Backend.loggedUser.UserID;

            PatchmanagerClient.DownloadComplete += (object sender, EventArgs e) => {
                loggedInAs.Dispatcher.Invoke(() => {
                    (Application.Current.MainWindow as MainWindow).playButton.IsEnabled = true;
                    (Application.Current.MainWindow as MainWindow).playButton.Opacity = 1;
                });
            };


            if (Backend.PatchData != null && Backend.PatchData?.RemainingSize == 0) {
                (Application.Current.MainWindow as MainWindow).playButton.IsEnabled = true;
                (Application.Current.MainWindow as MainWindow).playButton.Opacity = 1;
            }
        }

        private async void Logout_Button_Clicked (object sender, RoutedEventArgs e) {
            Backend.Logout();
            (Application.Current.MainWindow as MainWindow).playButton.IsEnabled = false;
            (Application.Current.MainWindow as MainWindow).playButton.Opacity = .5;
            Settings.Default.SessionToken = "";
            Settings.Default.username = "";
            Settings.Default.RememberUsername = false;
            Settings.Default.AutoLogin = false;
            Settings.Default.Save();

            await AnimateOut();
            (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoginPage());
        }

        private void Play_Button_Click (object sender, RoutedEventArgs e) {
            Backend.LaunchGame();
        }
    }
}
