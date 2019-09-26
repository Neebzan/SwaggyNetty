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

namespace Launcher {
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page {

        TextBox usernameBox;
        PasswordBox passwordBox, confirmPassBox;


        public RegisterPage () {
            InitializeComponent();

            usernameBox = username_textbox;
            passwordBox = password_textbox;
            confirmPassBox = password_confirm_textbox;


        }

        private void Create_Account_Button_Clicked (object sender, RoutedEventArgs e) {
            //if (!string.IsNullOrEmpty(passwordBox.Password) && !string.IsNullOrEmpty(usernameBox.Text) && !string.IsNullOrEmpty(confirmPassBox.Password)) {
            //    SecureString password = passwordBox.SecurePassword;
            //    string username = usernameBox.Text;

            //    await Backend.SendLoginCredentials(username, password);
            //}
            //else {
            //    errorPopup.IsOpen = true;
            //    errorPopupMessage.Text = "Could not login.\n\nYou must fill out both username and password entries.";
            //}








            Application.Current.MainWindow.Content = new LoginPage();
        }
    }
}
