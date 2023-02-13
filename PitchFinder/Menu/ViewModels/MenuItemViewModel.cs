using System.Collections.Generic;
using System.Windows.Input;
using PitchFinder.ViewModels;

namespace PitchFinder.Menu
{
    internal class MenuItemViewModel : ViewModelBase, IMenuItem
    {
        public string Header { get; set; }
        public bool IsCheckable { get; set; }
        public List<IMenuItem> Items { get; set; }
        public ICommand Command { get; set; }

        public MenuItemViewModel()
        {
            Items = new List<IMenuItem>();
        }
        public MenuItemViewModel(string header) : this()
        {
            Header = header;
        }
        public MenuItemViewModel(string header, bool isCheckable) : this(header)
        {
            IsCheckable = isCheckable;
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }
    }
}
