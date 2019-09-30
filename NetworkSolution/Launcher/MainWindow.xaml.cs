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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
<<<<<<< HEAD
    public partial class MainWindow : Window {
=======
    public partial class MainWindow : Window
    {
        public Frame mainFrame;
        public Button playButton;
>>>>>>> parent of 7e1d543... Updated UI

        TextBox usernameBox;
        PasswordBox passwordBox;

        public MainWindow () {
            InitializeComponent();

<<<<<<< HEAD
            usernameBox = username_textbox;
            passwordBox = password_textbox;
        }

        private async void Button_Click (object sender, RoutedEventArgs e) {
            SecureString password = passwordBox.SecurePassword;
            string username = usernameBox.Text;

            await Backend.SendLoginCredentials(username, password);
=======
            mainFrame = frame;
            playButton = play_button;
            playButton.IsEnabled = false;
            playButton.Opacity = .5;
            //this.Content = new LoginPage();
            mainFrame.NavigationService.Navigate(new LoginPage());
>>>>>>> parent of 7e1d543... Updated UI
        }
    }
}
