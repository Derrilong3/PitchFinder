using System;
using System.Linq;
using System.Windows.Controls;

namespace PitchFinder.Models
{
    public interface IModule
    {
        string Name { get; }
        UserControl UserInterface { get; }
        void Deactivate();
    }
}
