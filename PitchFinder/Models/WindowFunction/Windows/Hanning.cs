using System;

namespace PitchFinder.Models.Windows
{
    public class Hanning : Window, IWindow
    {
        public override string Name => "Hanning";
        public override string Description =>
            "The Hanning window has a sinusoidal shape which touches zero at the edges (unlike the similar Hamming window). " +
            "It has good frequency resolution, low spectral leakage, and is satisfactory for 95 percent of use cases. " +
            "If you do not know the nature of the signal but you want to apply a smoothing window, start with the Hann window.";

        public override double Apply(int n, int size)
        {
            return 0.5 - 0.5 * Math.Cos(2 * Math.PI * n / (size - 1));
        }
    }
}