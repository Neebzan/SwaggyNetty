using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Launcher
{
    /// <summary>
    /// Interaction logic for LoggedInPage.xaml
    /// </summary>
    public partial class LoggedInPage : Page
    {
        Label loggedInAs;

        public LoggedInPage()
        {
            InitializeComponent();

            loggedInAs = logged_in_as_label;
            loggedInAs.Content += " " + Backend.loggedUser.UserID;
        }

        private void Logout_Button_Clicked(object sender, RoutedEventArgs e) {
            Backend.Logout();
            Application.Current.MainWindow.Content = new LoginPage();
        }

    }
}
