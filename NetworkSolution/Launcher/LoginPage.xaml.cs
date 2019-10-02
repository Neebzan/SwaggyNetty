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

namespace Launcher {
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : BasePage {
        TextBox usernameBox;
        PasswordBox passwordBox;
        CheckBox rememberUsername, automaticLogin;
        ImageAwesome spinner;

        string savedUsername;
        //SecureString savedPassword;

        public LoginPage () {
            InitializeComponent();

            usernameBox = username_textbox;
            passwordBox = password_textbox;
            rememberUsername = remember_username_tick;
            automaticLogin = automatic_login_tick;
            spinner = spinner_imageawesome;

            spinner.Visibility = Visibility.Hidden;
            this.Loaded += CheckAutoLogin;


            savedUsername = Settings.Default.username;
            rememberUsername.IsChecked = Settings.Default.RememberUsername;
            automaticLogin.IsChecked = Settings.Default.AutoLogin;
            if (rememberUsername.IsChecked == true && !string.IsNullOrEmpty(savedUsername)) {
                usernameBox.Text = savedUsername;
            }
        }

        private async void CheckAutoLogin (object sender, RoutedEventArgs e) {
            if (automaticLogin.IsChecked == true) {
                if (!string.IsNullOrEmpty(Settings.Default.SessionToken)) {
                    await AnimateOut();
                    SecureString password = passwordBox.SecurePassword;
                    string username = usernameBox.Text;
                    bool? rememberUsername = remember_username_tick.IsChecked;
                    (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoggingYouInPage(password, username, rememberUsername, true));
                }
            }
        }

        private void Login_Button_Clicked (object sender, RoutedEventArgs e) {
            spinner.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(passwordBox.Password) && !string.IsNullOrEmpty(usernameBox.Text)) {
                SecureString password = passwordBox.SecurePassword;
                string username = usernameBox.Text;
                bool? rememberUsername = remember_username_tick.IsChecked;

                Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(async () => {
                        await AnimateOut();
                        (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoggingYouInPage(password, username, rememberUsername, false));
                    }));
            }
            else {
                (Application.Current.MainWindow as MainWindow).DisplayError("Login request failed", "You must enter both a password and a username in order to login");
                spinner.Visibility = Visibility.Hidden;
            }
        }

        private void Remember_username_tick_Unchecked (object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(savedUsername)) {
                savedUsername = string.Empty;
            }
            Settings.Default.username = string.Empty;
            Settings.Default.RememberUsername = false;
            Settings.Default.Save();
            automaticLogin.IsChecked = false;
        }

        private void Remember_username_tick_Checked (object sender, RoutedEventArgs e) {
            Settings.Default.RememberUsername = true;
            Settings.Default.Save();
        }

        private void Automatic_login_tick_Unchecked (object sender, RoutedEventArgs e) {
            Settings.Default.AutoLogin = false;
            rememberUsername.IsEnabled = true;
            Settings.Default.Save();
        }

        private void Automatic_login_tick_Checked (object sender, RoutedEventArgs e) {
            Settings.Default.AutoLogin = true;
            rememberUsername.IsChecked = true;
            rememberUsername.IsEnabled = false;
            Settings.Default.Save();
        }

        private async void Register_Button_Clicked (object sender, RoutedEventArgs e) {
            await AnimateOut();
            (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new RegisterPage());
        }
    }
}
