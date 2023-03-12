using System;

namespace PitchFinder.Models.Windows
{
    public class Bartlett : Window, IWindow
    {
        public override string Name => "Bartlett";
        public override string Description =>
            "The Bartlett window is triangular in shape (a 2nd order B-spline) which is effectively the " +
            "convolution of two half-sized rectangular windows.";

        public override double Apply(int n, int size)
        {
            bool isOddSize = size % 2 == 1;

            double halfSize = isOddSize ? size / 2 : (size - 1) / 2.0;

            return 1 - Math.Abs((double)(n - halfSize) / halfSize);
        }
    }
}
