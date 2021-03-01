using System.Diagnostics;
using System.Windows;
using ValheimToolerLauncher.Properties;

namespace ValheimToolerLauncher
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (Debugger.IsAttached)
                Settings.Default.Reset();

        }
    }
}
