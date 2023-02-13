using PitchFinder.ViewModels;
using System.Collections.Generic;

namespace PitchFinder.Menu.MenuItems
{
    internal class ToolsMenu : MenuItemViewModel
    {
        public ToolsMenu(IEnumerable<ToolViewModel> dockWindows, string header = "Tools") : base(header)
        {
            foreach (var dockWindow in dockWindows)
                Items.Add(GetMenuItemViewModel(dockWindow));
        }

        private MenuItemViewModel GetMenuItemViewModel(ToolViewModel dockWindowViewModel)
        {
            var menuItemViewModel = new MenuItemViewModel(dockWindowViewModel.Title, true);
            menuItemViewModel.IsChecked = dockWindowViewModel.IsVisible;

            dockWindowViewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(ToolViewModel.IsVisible))
                    menuItemViewModel.IsChecked = dockWindowViewModel.IsVisible;
            };

            menuItemViewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(MenuItemViewModel.IsChecked))
                    dockWindowViewModel.IsVisible = menuItemViewModel.IsChecked;
            };

            return menuItemViewModel;
        }
    }
}
