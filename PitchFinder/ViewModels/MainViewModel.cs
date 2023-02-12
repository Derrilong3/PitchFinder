using PitchFinder.Models;
using System.Collections.Generic;

namespace PitchFinder.ViewModels
{
    internal class MainViewModel
    {
        public MenuViewModel MenuViewModel { get; private set; }
        public IEnumerable<ToolViewModel> Anchorables { get; private set; }

        public MainViewModel()
        {
            Anchorables = new List<ToolViewModel>()
            {
                new PlotViewModel()
            };

            MenuViewModel = new MenuViewModel(Anchorables);
        }
    }
}
