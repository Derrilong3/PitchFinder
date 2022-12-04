using PitchFinder.ViewModels;
using PitchFinder.Views;
using System.Windows;

namespace PitchFinder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //AllocConsole();
            var mainWindow = new MainWindow();
            var vm = new MainViewModel();
            mainWindow.DataContext = vm;
            mainWindow.Show();
        }
    }
}
