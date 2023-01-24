using System.Collections.Generic;

namespace PitchFinder.ViewModels
{
    internal class MenuViewModel
    {
        public IEnumerable<MenuItemViewModel> Items { get; private set; }

        private readonly MenuItemViewModel ViewMenuItemViewModel;

        public MenuViewModel(IEnumerable<ToolViewModel> dockWindows)
        {
            var view = this.ViewMenuItemViewModel = new MenuItemViewModel() { Header = "Tools" };

            foreach (var dockWindow in dockWindows)
                view.Items.Add(GetMenuItemViewModel(dockWindow));

            var items = new List<MenuItemViewModel>();
            items.Add(view);
            this.Items = items;
        }

        private MenuItemViewModel GetMenuItemViewModel(ToolViewModel dockWindowViewModel)
        {
            var menuItemViewModel = new MenuItemViewModel();
            menuItemViewModel.IsCheckable = true;

            menuItemViewModel.Header = dockWindowViewModel.Title;
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
