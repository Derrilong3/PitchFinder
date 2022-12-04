using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using PitchFinder.Models;
using PitchFinder.Views;
using PitchFinder.ViewModels;

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
