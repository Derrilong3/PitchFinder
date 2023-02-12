﻿
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

        public double[] GetNotesMulti(List<Tuple<double, double>> tuples, int maxOctava = 6)
        {    
            double[] output = new double[noteBaseFreqs.Count];
            double sum;
            double noteFreq;
            double curFreq;
            double OnePercent;

            for (int i = 0; i < output.Length; i++)
            {
                sum = 0;
                noteFreq = noteBaseFreqs.ElementAt(i).Value;              
                for (int j = 0; j < maxOctava; j++)
                {
                    curFreq = noteFreq * Math.Pow(2, j);
                    OnePercent = curFreq * 0.01d;
                    var array = tuples.FindAll(x => curFreq - OnePercent < x.Item1 && x.Item1 < curFreq + OnePercent).MaxBy(x => x.Item2);

                    if (array != null)
                    {
                        sum += array.Item2;
                    }
                }

                output[i] = sum;
            }

            return output;
        }

        public event EventHandler<FftEventArgs> FftCalculated;

        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

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
                var dd = new AudioFileReader(fileName);
                FileStream = inputStream;
                var aggregator = new SampleAggregator(inputStream);
                SampleRate = inputStream.WaveFormat.SampleRate;
                aggregator.NotificationCount = inputStream.WaveFormat.SampleRate / 100;
                aggregator.PerformFFT = true;
                aggregator.FftCalculated += (s, a) => FftCalculated?.Invoke(this, a);
                aggregator.MaximumCalculated += (s, a) => MaximumCalculated?.Invoke(this, a);
                aggregator.BufferEventArgs += (s, a) => BufferEventArgs?.Invoke(this, a);
                PlaybackDevice.Init(aggregator);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem opening file");
                CloseFile();
            }
        }

        public string GetNote(float freq)
        {
            float baseFreq;

            foreach (var note in noteBaseFreqs)
            {
                baseFreq = note.Value;

                for (int i = 0; i < 9; i++)
                {
                    if (freq >= baseFreq - 3 && freq < baseFreq + 3 || freq == baseFreq)
                    {
                        return note.Key + i;
                    }

                    baseFreq *= 2;
                }
            }

            return null;
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
