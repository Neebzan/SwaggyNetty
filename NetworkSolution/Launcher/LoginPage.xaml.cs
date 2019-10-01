using FontAwesome.WPF;
using Launcher.Properties;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : BasePage
    {
        TextBox usernameBox;
        PasswordBox passwordBox;
        Popup errorPopup;
        TextBlock errorPopupMessage;
        CheckBox rememberUsername, automaticLogin;
        ImageAwesome spinner;

        string savedUsername;
        //SecureString savedPassword;

        public LoginPage () {
            InitializeComponent();

            usernameBox = username_textbox;
            passwordBox = password_textbox;
            errorPopup = Error_Popup;
            errorPopupMessage = Error_Popup_Label;
            rememberUsername = remember_username_tick;
            automaticLogin = automatic_login_tick;
            spinner = spinner_imageawesome;
            spinner.Visibility = Visibility.Hidden;
            Application.Current.MainWindow.Deactivated += (object sender, EventArgs e) => {
                errorPopup.IsOpen = false;
            };

            Application.Current.MainWindow.LocationChanged += (object sender, EventArgs e) => {
                errorPopup.IsOpen = false;
            };
            this.usernameBox.KeyDown += (object sender, KeyEventArgs e) => {
                errorPopup.IsOpen = false;
            };

            errorPopup.IsOpen = false;

            savedUsername = Settings.Default.username;
            rememberUsername.IsChecked = Settings.Default.RememberUsername;
            automaticLogin.IsChecked = Settings.Default.AutoLogin;
            if (!string.IsNullOrEmpty(savedUsername)) {
                rememberUsername.IsChecked = true;
                usernameBox.Text = savedUsername;
            }

            this.Loaded += CheckAutoLogin;

        }

        private async void CheckAutoLogin (object sender, RoutedEventArgs e) {
            if (automaticLogin.IsChecked == true) {
                if (!string.IsNullOrEmpty(Settings.Default.SessionToken)) {
                    await AnimateOut();
                    (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoggingYouInPage());
                }
            }
        }

        private async void LoginRequest (SecureString password, string username, bool? rememberUsername) {
            try {
                if (await Backend.SendLoginCredentials(username, password)) {
                    if (rememberUsername == true) {
                        Settings.Default.username = Backend.loggedUser.UserID;
                        Settings.Default.SessionToken = Backend.loggedUser.Token;
                        Settings.Default.Save();
                    }

                    Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(async () => {
                        await AnimateOut();
                        (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoggedInPage());
                    }));
                }
            }
            catch (Exception e) {
                Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(() => {
                    errorPopup.IsOpen = true;
                    Error_Popup_Label.Text = "Could not login.\n\n" + e.Message;
                }));
            }
        }

        private void Login_Button_Clicked (object sender, RoutedEventArgs e) {
            spinner.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(passwordBox.Password) && !string.IsNullOrEmpty(usernameBox.Text)) {
                SecureString password = passwordBox.SecurePassword;
                string username = usernameBox.Text;
                bool? rememberUsername = remember_username_tick.IsChecked;

                Task.Factory.StartNew(() => LoginRequest(password, username, rememberUsername));
            }
            else {
                errorPopup.IsOpen = true;
                errorPopupMessage.Text = "Could not login.\n\nYou must fill out both username and password entries.";
                spinner.Visibility = Visibility.Hidden;
            }
        }

       

        private void Remember_username_tick_Unchecked (object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(savedUsername)) {
                savedUsername = string.Empty;
                Settings.Default.username = string.Empty;
                Settings.Default.RememberUsername = false;
                Settings.Default.Save();
            }
        }

        private void Remember_username_tick_Checked (object sender, RoutedEventArgs e) {
            Settings.Default.RememberUsername = true;
            Settings.Default.Save();
        }

        private void Automatic_login_tick_Unchecked (object sender, RoutedEventArgs e) {
            Settings.Default.AutoLogin = false;
            Settings.Default.Save();
        }

        private void Automatic_login_tick_Checked (object sender, RoutedEventArgs e) {
            Settings.Default.AutoLogin = true;
            Settings.Default.Save();
        }

        private async void Register_Button_Clicked (object sender, RoutedEventArgs e) {
            await AnimateOut();
            (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new RegisterPage());
        }
    }
}
