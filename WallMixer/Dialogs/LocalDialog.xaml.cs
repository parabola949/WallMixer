namespace WallMixer.Dialogs
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using DTO;
    using MahApps.Metro.Controls.Dialogs;

    /// <summary>
    /// Interaction logic for WallhavenDialog.xaml
    /// </summary>
    public partial class LocalDialog : ICustomDialog
    {
        public string Message { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
        

        public LocalDialog(string message)
        {
            Message = message;
            InitializeComponent();
        }

        public Task<WallpaperSource> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                QueryTextbox.Focus();
            }));

            var tcs = new TaskCompletionSource<WallpaperSource>();
            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;
            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;
            KeyEventHandler escapeKeyHandler = null;
            Action cleanUpHandlers = null;

            cleanUpHandlers = () =>
            {
                QueryTextbox.KeyDown -= affirmativeKeyHandler;
                this.KeyDown -= escapeKeyHandler;
                NegativeButton.Click -= negativeHandler;
                AffirmativeButton.Click -= affirmativeHandler;
                NegativeButton.KeyDown -= negativeKeyHandler;
                AffirmativeButton.KeyDown -= affirmativeKeyHandler;
            };

            escapeKeyHandler = (sender, e) =>
            {
                if (e.Key != Key.Escape) return;
                cleanUpHandlers();
                tcs.TrySetResult(null);
            };

            negativeKeyHandler = (sender, e) =>
            {
                if (e.Key != Key.Enter) return;
                cleanUpHandlers();
                tcs.TrySetResult(null);
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key != Key.Enter) return;
                cleanUpHandlers();
                tcs.TrySetResult(GetResult());
            };

            negativeHandler = (sender, e) =>
            {
                cleanUpHandlers();
                tcs.TrySetResult(null);
                e.Handled = true;
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();
                tcs.TrySetResult(GetResult());
                e.Handled = true;
            };

            NegativeButton.KeyDown += negativeKeyHandler;
            AffirmativeButton.KeyDown += affirmativeKeyHandler;
            QueryTextbox.KeyDown += affirmativeKeyHandler;
            this.KeyDown += escapeKeyHandler;
            NegativeButton.Click += negativeHandler;
            AffirmativeButton.Click += affirmativeHandler;
            return tcs.Task;
        }

        private WallpaperSource GetResult()
        {
            return new WallpaperSource
            {
                Query = this.Query,
                Name = Name,
                Source = Source.Local
            };
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }
    }
}
