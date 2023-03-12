﻿using System;

namespace PitchFinder.Models.Windows
{
    public class Cosine : Window, IWindow
    {
        public override string Name => "Cosine";
        public override string Description =>
            "This window is simply a cosine function. It reaches zero on both sides and is similar to " +
            "Blackman, Hamming, Hanning, and flat top windows, but probably should not be used in practice.";

        public override double Apply(int n, int size)
        {
            return Math.Sin(Math.PI * n / (size - 1));
        }
    }
}