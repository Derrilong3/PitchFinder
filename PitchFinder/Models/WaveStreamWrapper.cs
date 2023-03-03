using NAudio.Wave;
using SoundTouch.Net.NAudioSupport;
using System;

namespace PitchFinder.Models
{
    internal class WaveStreamWrapper : IDisposable
    {
        public WaveStream WaveStream { get; set; }
        public SoundTouchWaveProvider SoundTouchProvider { get; set; }

        public void Init(WaveStream waveStream)
        {
            WaveStream = waveStream;
            SoundTouchProvider = new SoundTouchWaveProvider(waveStream);
            SoundTouchProvider.Tempo = Tempo;
        }

        private double _tempo;
        public double Tempo
        {
            get => _tempo;
            set
            {
                if (_tempo == value)
                    return;

                _tempo = Math.Round(value, 2);

                if (SoundTouchProvider != null)
                {
                    SoundTouchProvider.Tempo = _tempo;
                }
            }
        }

        public void Dispose()
        {
            WaveStream?.Dispose();
            WaveStream = null;
            SoundTouchProvider = null;
        }
    }
}
