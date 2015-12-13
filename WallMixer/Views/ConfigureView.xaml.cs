using System.Windows;

namespace WallMixer.Views
{
    /// <summary>
    /// Interaction logic for ConfigureView.xaml
    /// </summary>
    public partial class ConfigureView
    {
        public ConfigureView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            LeftClickMenu.PlacementTarget = this;
            LeftClickMenu.IsOpen = true;
        }
    }
}
