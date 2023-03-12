using System;

namespace PitchFinder.Models.Windows
{
    public class FlatTop : Window, IWindow
    {
        public override string Name => "Flat Top";
        public override string Description =>
            "A flat top window is a partially negative-valued window that has minimal scalloping loss in the frequency domain. " +
            "These properties are desirable for the measurement of amplitudes of sinusoidal frequency components. " +
            "Drawbacks of the broad bandwidth are poor frequency resolution and high noise bandwidth. " +
            "The flat top window crosses the zero line causing a broader peak in the frequency domain, " +
            "which is closer to the true amplitude of the signal than with other windows";

        public override double Apply(int n, int size)
        {
            return 0.21557895
                   - 0.41663158 * Math.Cos(2 * Math.PI * n / (size - 1))
                   + 0.277263158 * Math.Cos(4 * Math.PI * n / (size - 1))
                   - 0.083578947 * Math.Cos(6 * Math.PI * n / (size - 1))
                   + 0.006947368 * Math.Cos(8 * Math.PI * n / (size - 1));
        }
    }
}
