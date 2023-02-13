using PitchFinder.Menu;
using PitchFinder.Menu.MenuItems;
using System.Collections.Generic;

namespace PitchFinder.ViewModels
{
    internal class MenuViewModel
    {
        public IEnumerable<MenuItemViewModel> Items { get; private set; }

        public MenuViewModel(IEnumerable<ToolViewModel> dockWindows)
        {
            Items = new List<MenuItemViewModel>
            {
                new ToolsMenu(dockWindows),
                new WindowFuncMenu()
            };
        }
    }
}
