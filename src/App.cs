using System.Configuration;
using System.Data;
using System.Windows;

namespace NodifyTestWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            var window = new MainWindow();
            app.Run(window);
        }
    }

}
