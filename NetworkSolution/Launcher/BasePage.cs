using Launcher.Animations;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Launcher
{
    public class BasePage : Page
    {
        public PageAnimation PageLoadAnimation { get; set; } = PageAnimation.SlideAndFadeInFromRight;
        public PageAnimation PageUnloadAnimation { get; set; } = PageAnimation.SlideAndFadeOutToLeft;

        public float SlideSeconds { get; set; } = .8f;

        public BasePage () {
            if (this.PageLoadAnimation != PageAnimation.None) {
                this.Visibility = Visibility.Collapsed;
            }

            this.Loaded += BasePage_Loaded;
        }

        private void BasePage_Loaded (object sender, RoutedEventArgs e) {
            AnimateIn();
        }

        private void AnimateIn () {
            throw new NotImplementedException();
        }
    }
}
