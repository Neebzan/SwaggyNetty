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

namespace Launcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        TextBox usernameBox;
        PasswordBox passwordBox;
        Popup errorPopup;
        TextBlock errorPopupMessage;

        public MainWindow () {
            InitializeComponent();

            usernameBox = username_textbox;
            passwordBox = password_textbox;
            errorPopup = Error_Popup;
            errorPopupMessage = Error_Popup_Label;

            Deactivated += (object sender, EventArgs e) => {
                errorPopup.IsOpen = false;
            };

            LocationChanged += (object sender, EventArgs e) => {
                errorPopup.IsOpen = false;
            };
            this.usernameBox.KeyDown += (object sender, KeyEventArgs e) => {
                errorPopup.IsOpen = false;
            };

            errorPopup.IsOpen = false;
        }

        private async void Login_Button_Clicked (object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(passwordBox.Password) && !string.IsNullOrEmpty(usernameBox.Text)) {
                SecureString password = passwordBox.SecurePassword;
                string username = usernameBox.Text;

                await Backend.SendLoginCredentials(username, password);
            }
            else {
                errorPopup.IsOpen = true;
                errorPopupMessage.Text = "You must fill out both username and password entries.";
            }
        }

        private void Register_Button_Clicked (object sender, RoutedEventArgs e) {
            
        }
    }
}
