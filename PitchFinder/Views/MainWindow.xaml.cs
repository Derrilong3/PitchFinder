using PitchFinder.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows;

namespace PitchFinder.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var arch = Environment.Is64BitProcess ? "x64" : "x86";
            var framework = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            this.Title = $"{this.Title} ({framework}) ({arch})";

            this.DataContext = new MainViewModel();

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Closing += new CancelEventHandler(MainWindow_Unloaded);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
            serializer.LayoutSerializationCallback += (s, args) =>
            {
                args.Content = args.Content;
            };

            if (File.Exists(@".\AvalonDock.config"))
                serializer.Deserialize(@".\AvalonDock.config");
        }

        private void MainWindow_Unloaded(object sender, CancelEventArgs e)
        {
            var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
            serializer.Serialize(@".\AvalonDock.config");
            Properties.Settings.Default.Save();
            ((MainViewModel)(DataContext)).Dispose();
        }
    }
}
