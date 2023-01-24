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
                new MediaPlaybackViewModel()
            };

            MenuViewModel = new MenuViewModel(Anchorables);
        }
    }
}
