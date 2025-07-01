using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;

namespace BrowserSelect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("shell32.dll", SetLastError = true)]
        static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        protected override void OnStartup(StartupEventArgs e)
        {
            if( e.Args.Any() && e.Args.Contains("--install"))
            {
                Program.Main2(e.Args);
                base.Shutdown();
                return;
            }
            base.OnStartup(e);
        }
    }

}
