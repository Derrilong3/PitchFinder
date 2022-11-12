using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using PitchFinder.Models;
using PitchFinder.Views;

namespace PitchFinder.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private IModule selectedModule;

        public MainWindowViewModel(IEnumerable<IModule> modules)
        {          
            Modules = modules.OrderBy(m => m.Name).ToList();
            if (Modules.Count > 0)
            {
                SelectedModule = Modules[0];
            }

        }

        public List<IModule> Modules { get; }

        public IModule SelectedModule
        {
            get => selectedModule;
            set
            {
                if (value != selectedModule)
                {
                    selectedModule?.Deactivate();
                    selectedModule = value;
                    OnPropertyChanged("SelectedModule");
                    OnPropertyChanged("UserInterface");
                }
            }
        }

        public UserControl UserInterface => SelectedModule?.UserInterface;
    }
}
