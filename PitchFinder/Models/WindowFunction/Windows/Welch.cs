using System;

namespace PitchFinder.Models.Windows
{
    public class Welch : Window, IWindow
    {
        public override string Name => "Welch";
        public override string Description =>
            "The Welch window is typically used for antialiasing and resampling. " +
            "It has a parabolic shape and is zero at the edges, " +
            "gradually increasing towards the center of the window.";

        public override double Apply(int n, int size)
        {
            return 1 - Math.Pow((n - (size - 1) / 2.0) / ((size - 1) / 2.0), 2);
        }
    }
}
