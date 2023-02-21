using CommunityToolkit.Mvvm.Messaging;
using PitchFinder.ViewModels;
using FftSharp;

namespace PitchFinder.Menu.MenuItems
{
    class WindowFuncMenu : MenuItemViewModel
    {
        private MenuItemViewModel SelectedFunc;

        public WindowFuncMenu(string header = "Window Functions") : base(header)
        {
            var windows = Window.GetWindows();
            foreach (var widow in Window.GetWindows())
                Items.Add(GetMenuItemViewModel(widow));

            int selectedIndex = Properties.Settings.Default.selectedFunc;
            SelectedFunc = (MenuItemViewModel)Items[selectedIndex];
            SelectedFunc.IsChecked = true;
            SelectedFunc.IsCheckable = false;
        }

        private MenuItemViewModel GetMenuItemViewModel(IWindow window)
        {
            var menuItemViewModel = new MenuItemViewModel(window.Name, true);
            menuItemViewModel.IsChecked = false;
            menuItemViewModel.Command = new RelayCommand(() =>
            {
                SelectedFunc.IsCheckable = true;
                SelectedFunc.IsChecked = false;
                menuItemViewModel.IsCheckable = false;
                menuItemViewModel.IsChecked = true;

                SelectedFunc = menuItemViewModel;

                int idx = Items.IndexOf(SelectedFunc);
                Properties.Settings.Default.selectedFunc = idx;
                WeakReferenceMessenger.Default.Send(new Models.Messages.WindowFuncChangedMessage(idx));

            }, () => menuItemViewModel.IsCheckable);

            return menuItemViewModel;
        }
    }
}
