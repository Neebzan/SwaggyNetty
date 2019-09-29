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
using System.Windows.Media.Animation;
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
            frame.NavigationService.Navigate(new LoginPage());
            //Application.Current.MainWindow.Content = new LoginPage();
        }

        private void Play_Button_Click (object sender, RoutedEventArgs e) {
            Backend.LaunchGame();
        }

        private void Frame_Navigating (object sender, NavigatingCancelEventArgs e) {
            var ta = new ThicknessAnimation();
            ta.Duration = TimeSpan.FromSeconds(0.3);
            ta.DecelerationRatio = 0.7;
            ta.To = new Thickness(0, 0, 0, 0);
            if (e.NavigationMode == NavigationMode.New) {
                ta.From = new Thickness(500, 0, 0, 0);
            }

            else if (e.NavigationMode == NavigationMode.Back) {
                ta.From = new Thickness(0, 0, 500, 0);
            }

            //(e.Content as Page).BeginAnimation(MarginProperty, ta);
        }
    }
}
