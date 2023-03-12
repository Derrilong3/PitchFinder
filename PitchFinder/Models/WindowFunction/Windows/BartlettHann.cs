using System;

namespace PitchFinder.Models.Windows
{
    public class BartlettHann : Window, IWindow
    {
        public override string Name => "Bartlett-Hann";
        public override string Description =>
            "Combination of the Bartlett and Hann windows.";

        public override double Apply(int n, int size)
        {
            return 0.62 - 0.48 * Math.Abs(n / (size - 1) - 0.5) - 0.38 * Math.Cos(2 * Math.PI * n / (size - 1));
        }
    }
}
