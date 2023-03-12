using System;

namespace PitchFinder.Models.Windows
{
    public class BlackmanHarris : Window, IWindow
    {
        public override string Name => "Blackman-Harris";
        public override string Description =>
            "The Blackman-Harris window is a variant of the Blackman window that offers improved sidelobe attenuation compared to the standard Blackman window. " +
            "it has a wider main lobe than the standard Blackman window.";

        public override double Apply(int n, int size)
        {
            return 0.35875 - (0.48829 * Math.Cos((2 * Math.PI * n) / (size - 1))) + (0.14128 * Math.Cos((4 * Math.PI * n) / (size - 1))) - (0.01168 * Math.Cos((6 * Math.PI * n) / (size - 1)));
        }
    }
}