using Launcher.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Launcher
{
    public class ProgressBarValueChangedEventArgs : EventArgs
    {
        public float OldValue { get; set; }
        public float NewValue { get; set; }

        public ProgressBarValueChangedEventArgs (float _OldValue, float _NewValue) {
            OldValue = _OldValue;
            NewValue = _NewValue;
        }
    }

    class BaseProgressBar : ProgressBar
    {
        public EventHandler<ProgressBarValueChangedEventArgs> NewValueGiven;

        public BaseProgressBar () => this.NewValueGiven += BaseProgressBarValueChanged;



        private async void BaseProgressBarValueChanged (object sender, ProgressBarValueChangedEventArgs e) {
            await AnimateBar(e.OldValue, e.NewValue);                        
        }

        private async Task AnimateBar (double oldValue, double newValue) {
            await this.AnimateValueChange((float)oldValue, (float)newValue, 1.0f);
        }
    }
}
