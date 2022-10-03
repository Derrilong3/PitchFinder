using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            var mainWindow = new MainWindow();

            var modules = ReflectionHelper.CreateAllInstancesOf<IModule>();

            var vm = new MainWindowViewModel(modules);
            mainWindow.DataContext = vm;
            mainWindow.Closing += (s, args) => vm.SelectedModule.Deactivate();
            mainWindow.Show();
        }

        static class ReflectionHelper
        {
            public static IEnumerable<T> CreateAllInstancesOf<T>()
            {
                return typeof(ReflectionHelper).Assembly.GetTypes()
                    .Where(t => typeof(T).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract && t.IsClass)
                    .Select(t => (T)Activator.CreateInstance(t));
            }
        }
    }
}
