using Launcher.Properties;
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
using System.Windows.Threading;

namespace Launcher {
    /// <summary>
    /// Interaction logic for LoggingYouInPage.xaml
    /// </summary>
    public partial class LoggingYouInPage : BasePage {
        public LoggingYouInPage () {
            InitializeComponent();

            Loaded += StartAutoLogin;
            spinner_imageawesome.Visibility = Visibility.Visible;            
        }

        private void StartAutoLogin (object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(() => LoginWithToken(Settings.Default.SessionToken));
        }

        private async void LoginWithToken (string sessionToken) {
            await Task.Delay(500);
            try {
                if (await Backend.SendTokenLogin(sessionToken)) {
                    Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(async () => {
                        await AnimateOut();
                        (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoggedInPage());
                    }));
                }
                else {
                    Settings.Default.SessionToken = "";
                    Settings.Default.Save();
                    Dispatcher.Invoke(DispatcherPriority.Background,
new Action(async () => {
    await AnimateOut();
    (Application.Current.MainWindow as MainWindow).mainFrame.NavigationService.Navigate(new LoginPage());
}));
                }
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
