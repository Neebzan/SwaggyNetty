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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Launcher {
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : BasePage {

        TextBox usernameBox;
        PasswordBox passwordBox, confirmPassBox;
        Popup errorPopup;
        TextBlock errorPopupMessage;


        public RegisterPage () {
            InitializeComponent();

            usernameBox = username_textbox;
            passwordBox = password_textbox;
            confirmPassBox = password_confirm_textbox;
            errorPopup = this.Error_Popup;
            errorPopupMessage = Error_Popup_Label;
            spinner_imageawesome.Visibility = Visibility.Hidden;


            Application.Current.MainWindow.Deactivated += (object sender, EventArgs e) => {
                errorPopup.IsOpen = false;
            };

            Application.Current.MainWindow.LocationChanged += (object sender, EventArgs e) => {
                errorPopup.IsOpen = false;
            };
            this.usernameBox.KeyDown += (object sender, KeyEventArgs e) => {
                errorPopup.IsOpen = false;
            };
            this.passwordBox.KeyDown += (object sender, KeyEventArgs e) => {
                errorPopup.IsOpen = false;
            };
            this.confirmPassBox.KeyDown += (object sender, KeyEventArgs e) => {
                errorPopup.IsOpen = false;
            };

            errorPopup.IsOpen = false;

        }

        private async void BackToLogin_Button_Clicked (object sender, RoutedEventArgs e) {
            await AnimateOut();
            (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoginPage());
        }


        private void Create_Account_Button_Clicked (object sender, RoutedEventArgs e) {
            spinner_imageawesome.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(passwordBox.Password) && !string.IsNullOrEmpty(usernameBox.Text) && !string.IsNullOrEmpty(confirmPassBox.Password)) {
                SecureString password = passwordBox.SecurePassword;
                SecureString confirmPass = confirmPassBox.SecurePassword;
                string username = usernameBox.Text;
                Task.Factory.StartNew(() => CreateUserRequest(password, confirmPass, username));
            }
            else {
                errorPopup.IsOpen = true;
                errorPopupMessage.Background = Brushes.Red;
                errorPopupMessage.Text = "Could not create an account.\n\nYou must fill-out all fields in order to create an account.";
                spinner_imageawesome.Visibility = Visibility.Hidden;
            }
        }


        private async void CreateUserRequest (SecureString password, SecureString confirmPass, string username) {
            if (Backend.CheckPassUniformity(password, confirmPass)) {
                if (await Backend.SendRegisterRequest(username, password)) {
                    Settings.Default.username = username;
                    Settings.Default.Save();

                    Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(async () => {
                        await AnimateOut();
                        (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoginPage());
                    }));


                }
                else {
                    Dispatcher.Invoke(DispatcherPriority.Background,
                  new Action(() => {
                      this.errorPopup.IsOpen = true;
                      errorPopupMessage.Background = Brushes.Red;
                      errorPopupMessage.Text = "Could not create an account.\n\nSomething went wrong.";
                  }));
                }
            }
            else {
                Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(() => {
                    errorPopup.IsOpen = true;
                    errorPopupMessage.Background = Brushes.Red;
                    errorPopupMessage.Text = "Could not create an account.\n\nYour password does not match the confirmed password.";
                }));
            }
            Dispatcher.Invoke(DispatcherPriority.Background,
            new Action(() => {
                spinner_imageawesome.Visibility = Visibility.Hidden;
            }));

        }
    }
}
