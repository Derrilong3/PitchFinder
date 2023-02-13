using System.Collections.Generic;
using System.Windows.Input;

namespace PitchFinder.Menu
{
    internal interface IMenuItem
    {
        public string Header { get; set; }
        public bool IsCheckable { get; set; }
        public List<IMenuItem> Items { get; set; }
        public ICommand Command { get; set; }
        public bool IsChecked { get; set; }
    }
}
