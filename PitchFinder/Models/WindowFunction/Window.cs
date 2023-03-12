using System;
using System.Linq;
using System.Reflection;

namespace PitchFinder.Models
{
    public abstract class Window : IWindow
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public override string ToString() => Name;

        /// <summary>
        /// Multiply the array by this window and return the result as a new array
        /// </summary>
        public abstract double Apply(int n, int size);

        /// <summary>
        /// Return an array containing all available windows.
        /// Note that all windows returned will use the default constructor, but some
        /// windows have customization options in their constructors if you create them individually.
        /// </summary>
        public static IWindow[] GetWindows()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsClass)
                .Where(x => !x.IsAbstract)
                .Where(x => x.GetInterfaces().Contains(typeof(IWindow)))
                .Select(x => (IWindow)Activator.CreateInstance(x))
                .ToArray();
        }

        public static double[] Apply(double[] window, double[] signal)
        {
            if (window.Length != signal.Length)
                throw new ArgumentException("window and signal must be same length");

            double[] output = new double[window.Length];

            for (int i = 0; i < signal.Length; i++)
                output[i] = signal[i] * window[i];

            return output;
        }
    }
}
