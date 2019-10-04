using Launcher.Properties;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Launcher {
    /// <summary>
    /// Interaction logic for LoggingYouInPage.xaml
    /// </summary>
    public partial class LoggingYouInPage : BasePage {
        SecureString password;
        string username;
        bool? rememberUsername;
        bool autoLogin;

        public LoggingYouInPage (SecureString _password, string _username, bool? _rememberUsername, bool _autoLogin) {
            InitializeComponent();
            username = _username;
            password = _password;
            rememberUsername = _rememberUsername;
            autoLogin = _autoLogin;
            Loaded += StartLogin;
            spinner_imageawesome.Visibility = Visibility.Visible;
            this.PageUnloadAnimation = Animations.PageAnimation.FadeOut;
        }

        private void StartLogin (object sender, RoutedEventArgs e) {
            if (autoLogin)
                Task.Factory.StartNew(() => LoginWithToken(Settings.Default.SessionToken, Settings.Default.username));

            else
                Task.Factory.StartNew(() => LoginRequest(password, username, rememberUsername));
        }

        private async void LoginRequest (SecureString password, string username, bool? rememberUsername) {
            await Task.Delay(TimeSpan.FromSeconds(this.SlideSeconds));
            try {
                if (await Backend.SendLoginCredentials(username, password)) {
                    if (rememberUsername == true) {
                        Settings.Default.username = Backend.loggedUser.UserID;
                    }
                        Settings.Default.SessionToken = Backend.loggedUser.Token;
                        Settings.Default.Save();

                    Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(async () => {
                        await AnimateOut();
                        (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoggedInPage());
                    }));
                }
                else {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(async () => {
                        await AnimateOut();
                        (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoginPage());
                    }));
                }
            }
            catch (Exception e) {
                (Application.Current.MainWindow as MainWindow).DisplayError("Login request failed", e.Message);
            }
        }

        private async void LoginWithToken (string sessionToken, string userID) {
            await Task.Delay(500);
            try {
                if (await Backend.SendTokenLogin(sessionToken, userID)) {
                    Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(async () => {
                        await AnimateOut();
                        (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoggedInPage());
                    }));
                }
                else {
                    Settings.Default.SessionToken = "";
                    Settings.Default.AutoLogin = false;
                    Settings.Default.Save();
                    Dispatcher.Invoke(DispatcherPriority.Background,
new Action(async () => {
    await AnimateOut();
    (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoginPage());
}));
                }
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
