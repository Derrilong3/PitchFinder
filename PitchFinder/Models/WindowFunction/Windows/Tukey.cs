using System;

namespace PitchFinder.Models.Windows
{
    public class Tukey : Window, IWindow
    {
        public override string Name => "Tukey";
        public override string Description =>
            "A Tukey window has a flat center and tapers at the edges according to a cosine function. " +
            "The amount of taper is defined by alpha (with low values being less taper). " +
            "Tukey windows are ideal for analyzing transient data since the amplitude of transient signal " +
            "in the time domain is less likely to be altered compared to using Hanning or flat top.";

        public override double Apply(int n, int size)
        {
            double m = 2 * Math.PI / (0.5 * (size - 1));

            int edgeSizePoints = (int)(size * 0.5 / 2);

            if (size % 2 == 0)
                edgeSizePoints += 1;

            if (n < edgeSizePoints)
            {
                // left edge
                return (1 - Math.Cos(n * m)) / 2;
            }
            else if (n >= size - edgeSizePoints)
            {
                // right edge
                return (1 - Math.Cos(n * m)) / 2;
            }
            else
            {
                return 1;
            }
        }
    }
}
