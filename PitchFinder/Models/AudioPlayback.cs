
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PitchFinder.Models
{
    class AudioPlayback : IDisposable
    {
        public IWavePlayer PlaybackDevice { get; set; }
        public WaveStream FileStream { get; set; }
        public int SampleRate { get; set; }


        public static Dictionary<string, float> noteBaseFreqs = new Dictionary<string, float>()
            {
                { "C", 16.35f },
                { "C#", 17.32f },
                { "D", 18.35f },
                { "Eb", 19.45f },
                { "E", 20.60f },
                { "F", 21.83f },
                { "F#", 23.12f },
                { "G", 24.50f },
                { "G#", 25.96f },
                { "A", 27.50f },
                { "Bb", 29.14f },
                { "B", 30.87f },
            };

        public event EventHandler<BufferEventArgs> BufferEventArgs;
        public void Load(string fileName)
        {
            Stop();
            CloseFile();
            EnsureDeviceCreated();
            OpenFile(fileName);
        }

        public void CloseFile()
        {
            FileStream?.Dispose();
            FileStream = null;
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                FileStream = inputStream;
                var aggregator = new SampleAggregator(inputStream);
                SampleRate = inputStream.WaveFormat.SampleRate;
                aggregator.BufferEventArgs += (s, a) => BufferEventArgs?.Invoke(this, a);
                PlaybackDevice.Init(aggregator);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem opening file");
                CloseFile();
            }
        }

        private void EnsureDeviceCreated()
        {
            if (PlaybackDevice == null)
            {
                CreateDevice();
            }
        }

        public void CreateDevice()
        {
            PlaybackDevice = new WaveOut { DesiredLatency = 200 };
        }

        public void Play()
        {
            if (PlaybackDevice != null && FileStream != null && PlaybackDevice.PlaybackState != PlaybackState.Playing)
            {
                PlaybackDevice.Play();
            }
        }

        public void Pause()
        {
            PlaybackDevice?.Pause();
        }

        public void Stop()
        {
            PlaybackDevice?.Stop();
            if (FileStream != null)
            {
                FileStream.Position = 0;
            }
        }

        public void Dispose()
        {
            Stop();
            CloseFile();
            PlaybackDevice?.Dispose();
            PlaybackDevice = null;
        }
    }
}
