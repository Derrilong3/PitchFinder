using System;
using System.Linq;
using System.Windows.Controls;

namespace PitchFinder
{
    public interface IModule
    {
        string Name { get; }
        UserControl UserInterface { get; }
        void Deactivate();
    }
}
