using System;
using System.Collections.Generic;

namespace PitchFinder.ViewModels
{
    internal class MainViewModel : IDisposable
    {
        public MenuViewModel MenuViewModel { get; private set; }
        public IEnumerable<ToolViewModel> Anchorables { get; private set; }
        public MediaPlaybackViewModel ToolBarViewModel { get; private set; }

        public MainViewModel()
        {
            Anchorables = new List<ToolViewModel>()
            {
                new PlotViewModel()
            };

            MenuViewModel = new MenuViewModel(Anchorables);
            ToolBarViewModel = new MediaPlaybackViewModel();
        }

        public void Dispose()
        {
            ToolBarViewModel.Dispose();
        }
    }
}
