using System;

namespace PitchFinder.Models.Windows
{
    public class Blackman : Window, IWindow
    {
        public override string Name => "Blackman";
        public override string Description =>
            "The Blackman window is similar to Hamming and Hanning windows. " +
            "The resulting spectrum has a wide peak, but good side lobe compression.";

        public override double Apply(int n, int size)
        {
            return 0.42659071 - 0.49656062 * Math.Cos(2 * Math.PI * (double)n / (size - 1)) + 0.07684867 * Math.Cos(4 * Math.PI * (double)n / (size - 1));
        }
    }
}
