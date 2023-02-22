using NAudio.Wave;
using SoundTouch.Net.NAudioSupport;
using System;

namespace PitchFinder.Models
{
    internal class WaveStreamWrapper : IDisposable
    {
        public WaveStream WaveStream { get; set; }
        public SoundTouchWaveProvider SoundTouchProvider { get; set; }

        public void Dispose()
        {
            WaveStream?.Dispose();
        }
    }
}
