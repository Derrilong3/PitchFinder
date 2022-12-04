using PitchFinder.Views;
using System.Collections.Generic;

namespace PitchFinder.ViewModels
{
    internal class MainViewModel
    {
        public DockManagerViewModel DockManagerViewModel { get; private set; }
        public MenuViewModel MenuViewModel { get; private set; }

        public MainViewModel()
        {
            var documents = new List<DockWindowViewModel>()
            {
                new MediaPlaybackViewModel() {Title = "Media Window"}
            };

            this.DockManagerViewModel = new DockManagerViewModel(documents);
            this.MenuViewModel = new MenuViewModel(documents);
        }
    }
}
