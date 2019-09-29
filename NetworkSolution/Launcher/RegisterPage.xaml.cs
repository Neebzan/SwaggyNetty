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

namespace Launcher
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : BasePage
    {

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


        private async void Create_Account_Button_Clicked (object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(passwordBox.Password) && !string.IsNullOrEmpty(usernameBox.Text) && !string.IsNullOrEmpty(confirmPassBox.Password)) {
                SecureString password = passwordBox.SecurePassword;
                SecureString confirmPass = confirmPassBox.SecurePassword;
                string username = usernameBox.Text;

                if (Backend.CheckPassUniformity(password, confirmPass)) {
                    await Backend.SendRegisterRequest(username, password);
                }
                else {
                    errorPopup.IsOpen = true;
                    errorPopupMessage.Text = "Could not create an account.\n\nYour password does not match the confirmed password.";
                }

            }
            else {
                errorPopup.IsOpen = true;
                errorPopupMessage.Text = "Could not create an account.\n\nYou must fill-out all fields in order to create an account.";
            }
        }
    }
}
