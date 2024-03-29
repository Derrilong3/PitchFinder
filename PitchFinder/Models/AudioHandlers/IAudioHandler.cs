﻿using NAudio.Wave;
using System;

namespace PitchFinder.Models
{
    internal interface IAudioHandler : IDisposable
    {
        public double[] Samples { get; }
        public int SampleRate { get; }
        public bool IsPlaying { get; }
        public bool IsStopped { get; }

        public event EventHandler<StoppedEventArgs> PlaybackStopped;
        public event EventHandler DataReceived;

        public void Play();
        public void Stop();
        public void Pause();
        public bool Load();
    }
}
