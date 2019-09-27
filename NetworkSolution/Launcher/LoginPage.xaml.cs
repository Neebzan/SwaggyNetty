using FontAwesome.WPF;
using Launcher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
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
            if (!string.IsNullOrEmpty(savedUsername)) {
                rememberUsername.IsChecked = true;
                usernameBox.Text = savedUsername;
            }
            //savedPassword = Settings.Default.password;

            if (true) {

            }
        }


        private async void Login_Button_Clicked (object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(passwordBox.Password) && !string.IsNullOrEmpty(usernameBox.Text)) {
                SecureString password = passwordBox.SecurePassword;
                string username = usernameBox.Text;

                try {
                    spinner.Visibility = Visibility.Visible;
                    if (/*await Backend.SendLoginCredentials(username, password)*/true) {
                        if (rememberUsername.IsChecked == true) {
                            Settings.Default.username = username;
                            Settings.Default.Save();
                        }
                        Application.Current.MainWindow.Content = new LoggedInPage();
                    }

                }
                catch (Exception) {

                    throw;
                }
                spinner.Visibility = Visibility.Hidden;
            }
            else {
                errorPopup.IsOpen = true;
                errorPopupMessage.Text = "Could not login.\n\nYou must fill out both username and password entries.";
            }
        }

        private void Remember_username_tick_Unchecked (object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(savedUsername)) {
                savedUsername = string.Empty;
                Settings.Default.username = string.Empty;
            }
        }

        private void Register_Button_Clicked (object sender, RoutedEventArgs e) {
            //errorPopup.IsOpen = true;
            //errorPopupMessage.Text = "Could not create an account.\n\nThis feature is not yet implemented dum dum.";
            Application.Current.MainWindow.Content = new RegisterPage();
        }
    }
}
