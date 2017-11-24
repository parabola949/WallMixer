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
    public partial class WallhavenDialog : ICustomDialog
    {
        public string Message { get; set; }
        public string Query { get; set; }
        public bool General { get; set; }
        public bool Anime { get; set; }
        public bool People { get; set; }
        public bool SFW { get; set; }
        public bool Sketchy { get; set; }
        public ObservableCollection<SelectableObject<string>> Resolutions { get; set; }
        public ObservableCollection<SelectableObject<string>> Ratios { get; set; }

        public WallhavenDialog(string message)
        {
            Message = message;
            Resolutions = new ObservableCollection<SelectableObject<string>>
            {
                new SelectableObject<string>("1024x768"),
                new SelectableObject<string>("1280x800"),
                new SelectableObject<string>("1366x768"),
                new SelectableObject<string>("1280x960"),
                new SelectableObject<string>("1440x900"),
                new SelectableObject<string>("1600x900"),
                new SelectableObject<string>("1280x1024"),
                new SelectableObject<string>("1600x1200"),
                new SelectableObject<string>("1680x1050"),
                new SelectableObject<string>("1920x1080"),
                new SelectableObject<string>("1920x1200"),
                new SelectableObject<string>("2560x1440"),
                new SelectableObject<string>("2560x1600"),
                new SelectableObject<string>("3840x1080"),
                new SelectableObject<string>("5760x1080"),
                new SelectableObject<string>("3840x2160")
            };
            Ratios = new ObservableCollection<SelectableObject<string>>
            {
                new SelectableObject<string>("4x3"),
                new SelectableObject<string>("5x4"),
                new SelectableObject<string>("16x9"),
                new SelectableObject<string>("16x10"),
                new SelectableObject<string>("21x9"),
                new SelectableObject<string>("32x9"),
                new SelectableObject<string>("48x9")
            };
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
                Source = Source.Wallhaven,
                WallhavenOptions = new WallhavenOptions
                {
                    General = Convert.ToInt32(this.General),
                    Anime = Convert.ToInt32(this.Anime),
                    People = Convert.ToInt32(this.People),
                    SFW = Convert.ToInt32(this.SFW),
                    Sketchy = Convert.ToInt32(this.Sketchy),
                    Resolution = string.Join(",", Resolutions.Where(x => x.IsSelected).Select(x => x.ObjectData)),
                    Ratio = string.Join(",", Ratios.Where(x => x.IsSelected).Select(x => x.ObjectData))
                }
            };
        }


        private void OnChecked(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = (ComboBox) ItemsControl.ItemsControlFromItemContainer(FindParent<ComboBoxItem>((CheckBox) sender));
            BindingExpression bE = comboBox.GetBindingExpression(ComboBox.ItemsSourceProperty);
            StringBuilder sb = new StringBuilder();
            if (bE.ResolvedSourcePropertyName == "Resolutions")
                foreach (var selectableObject in Resolutions.Where(selectableObject => selectableObject.IsSelected))
                    sb.AppendFormat("{0},", selectableObject.ObjectData);
            if (bE.ResolvedSourcePropertyName == "Ratios")
                foreach (var selectableObject in Ratios.Where(selectableObject => selectableObject.IsSelected))
                    sb.AppendFormat("{0},", selectableObject.ObjectData);
            comboBox.Text = sb.ToString().Trim().TrimEnd(',');
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox) sender;
            comboBox.SelectedItem = null;
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
