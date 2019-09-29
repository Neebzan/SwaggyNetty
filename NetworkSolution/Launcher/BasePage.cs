using Launcher.Animations;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Launcher
{
    public class BasePage : Page
    {
        public PageAnimation PageLoadAnimation { get; set; } = PageAnimation.SlideAndFadeInFromLeft;
        public PageAnimation PageUnloadAnimation { get; set; } = PageAnimation.SlideAndFadeOutToRight;

        public float SlideSeconds { get; set; } = .6f;

        public BasePage () {
            if (this.PageLoadAnimation != PageAnimation.None) {
                this.Visibility = Visibility.Collapsed;
            }

            this.Loaded += BasePage_Loaded;
        }

        private async void BasePage_Unloaded (object sender, RoutedEventArgs e) {
            await AnimateOut();
        }
        private async void BasePage_Loaded (object sender, RoutedEventArgs e) {
            await AnimateIn();
        }

        public async Task AnimateOut () {
            switch (this.PageUnloadAnimation) {
                case PageAnimation.None:
                    return;
                case PageAnimation.SlideAndFadeInFromLeft:
                    await this.SlideAndFadeInFromLeft(this.SlideSeconds);
                    break;
                case PageAnimation.SlideAndFadeOutToRight:
                    await this.SlideAndFadeOutToRight(this.SlideSeconds);
                    break;
                default:
                    break;
            }
        }

        public async Task AnimateIn () {                

            switch (this.PageLoadAnimation) {
                case PageAnimation.None:
                        return;
                case PageAnimation.SlideAndFadeInFromLeft:
                    await this.SlideAndFadeInFromLeft(this.SlideSeconds);
                    break;
                case PageAnimation.SlideAndFadeOutToRight:
                    await this.SlideAndFadeOutToRight(this.SlideSeconds);
                    break;
                default:
                    break;
            }

        }


    }
}
