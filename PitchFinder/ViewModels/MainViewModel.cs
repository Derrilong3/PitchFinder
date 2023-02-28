using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PitchFinder.ViewModels
{
    internal class MainViewModel : IDisposable
    {
        public MenuViewModel MenuViewModel { get; private set; }
        public MediaPlaybackViewModel ToolBarViewModel { get; private set; }

        public MainViewModel()
        {
            MenuViewModel = new MenuViewModel(Anchorables);
            ToolBarViewModel = new MediaPlaybackViewModel();
        }

        private List<ToolViewModel> _tools;
        public List<ToolViewModel> Anchorables
        {
            get
            {
                if (_tools == null)
                {
                    _tools = new List<ToolViewModel>();

                    var types = Assembly.GetAssembly(typeof(ToolViewModel)).GetTypes().Where(myType => myType.IsSubclassOf(typeof(ToolViewModel)));

                    foreach (var type in types)
                    {
                        _tools.Add((ToolViewModel)Activator.CreateInstance(type));
                    }
                }

                return _tools;
            }
        }

        public void Dispose()
        {
            ToolBarViewModel.Dispose();
        }
    }
}
