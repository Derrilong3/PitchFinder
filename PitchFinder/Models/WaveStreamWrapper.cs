using NAudio.Wave;
using System;

namespace PitchFinder.Models
{
    internal class WaveStreamWrapper : IDisposable
    {
        public WaveStream WaveStream { get; set; }

        public void Dispose()
        {
            WaveStream?.Dispose();
        }
    }
}
