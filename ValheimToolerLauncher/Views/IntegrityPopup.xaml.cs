using System.Windows;

namespace ValheimToolerLauncher.Views
{
    /// <summary>
    /// Logique d'interaction pour IntegrityPopup.xaml
    /// </summary>
    public partial class IntegrityPopup
    {
        public IntegrityPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
