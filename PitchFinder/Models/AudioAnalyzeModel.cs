﻿using NAudio.Wave;
using OxyPlot;
using OxyPlot.Series;
using PitchFinder.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows.Media;

namespace PitchFinder.Models
{
    internal class AudioAnalyzeModel : ViewModelBase, IDisposable
    {
        private string lastPlayed;
        private double[] AudioValues;
        private AudioPlayback audioPlayback;
        private PlotModel plotModel;
        private readonly System.Timers.Timer timer;
        private float singleFrequency;
        private string singleNote;
        private bool timered;
        private Chromagram chromagram;

        public ObservableCollection<NoteBox> ColorMulti { get; private set; }
        public ObservableCollection<FftSharp.IWindow> WindowFunctions { get; private set; }

        public AudioAnalyzeModel()
        {
            audioPlayback = new AudioPlayback();
            audioPlayback.BufferEventArgs += audioGraph_Buffer;
            plotModel = new PlotModel();
            plotModel.Series.Add(new LineSeries());
            plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = 2000 });
            plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Maximum = 0.05f });

            ColorMulti = new ObservableCollection<NoteBox>();
            chromagram = new Chromagram();

            for (int i = 0; i < 12; i++)
            {
                ColorMulti.Add(new NoteBox() { Text = audioPlayback.noteBaseFreqs.ElementAt(i).Key, Color = Color.FromRgb(0, 0, 0) });
            }

            WindowFunctions = new ObservableCollection<FftSharp.IWindow>();

            foreach (FftSharp.IWindow window in FftSharp.Window.GetWindows())
            {
                WindowFunctions.Add(window);
            }

            WindowFunc = WindowFunctions[Properties.Settings.Default.selectedFunc];

            timer = new Timer(10);
            timer.Elapsed += TimerOnElapsed;
            timer.AutoReset = true;
        }

        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!timered)
            {
                return;
            }

            var s = (LineSeries)PlotModel.Series[0];

            s.Points.Clear();

            double[] windowed = WindowFunc.Apply(AudioValues);
            double[] paddedAudio = FftSharp.Pad.ZeroPad(AudioValues);
            double[] fftMag = FftSharp.Transform.FFTmagnitude(paddedAudio);
            double[] freq = FftSharp.Transform.FFTfreq(audioPlayback.SampleRate, fftMag.Length);

            try
            {
                for (int i = 0; i < freq.Length; i++)
                {

                    s.Points.Add(new DataPoint(freq[i], fftMag[i]));
                }
            }
            catch
            {
                return;
            }

            var chroma = chromagram.GetChroma(fftMag);
            for (int i = 0; i < ColorMulti.Count; i++)
            {
                byte G = (byte)(255 * chroma[i]);
                ColorMulti[i].Color = Color.FromRgb(0, G, 0);
            }

            //find the frequency peak
            int peakIndex = 0;
            for (int i = 0; i < fftMag.Length; i++)
            {
                if (fftMag[i] > fftMag[peakIndex])
                    peakIndex = i;
            }
            double fftPeriod = FftSharp.Transform.FFTfreqPeriod(audioPlayback.SampleRate, fftMag.Length);
            float peakFrequency = (float)Math.Round((fftPeriod * peakIndex) * 100f) / 100f;

            SingleFrequency = peakFrequency;
            SingleNote = audioPlayback.GetNote(peakFrequency);
            PlotModel.InvalidatePlot(true);
            timered = false;
        }

        void audioGraph_Buffer(object sender, BufferEventArgs e)
        {
            int bytesPerSamplePerChannel = audioPlayback.FileStream.WaveFormat.BitsPerSample / 8;
            int bytesPerSample = bytesPerSamplePerChannel * audioPlayback.FileStream.WaveFormat.Channels;
            int bufferSampleCount = e.Buffer.Length / bytesPerSample;

            if (bufferSampleCount >= AudioValues.Length)
            {
                bufferSampleCount = AudioValues.Length;
            }

            if (bytesPerSamplePerChannel == 2 && audioPlayback.FileStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt16(e.Buffer, i * bytesPerSample);
            }
            else if (bytesPerSamplePerChannel == 4 && audioPlayback.FileStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt32(e.Buffer, i * bytesPerSample);
            }
            else if (bytesPerSamplePerChannel == 4 && audioPlayback.FileStream.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToSingle(e.Buffer, i * bytesPerSample);
            }
            else
            {
                throw new NotSupportedException(audioPlayback.FileStream.WaveFormat.ToString());
            }

            timered = true;
        }

        public string InputPath { get; set; }

        public bool IsPlaying => audioPlayback.PlaybackDevice != null && audioPlayback.PlaybackDevice.PlaybackState == PlaybackState.Playing;

        public bool IsStopped => audioPlayback.PlaybackDevice == null || audioPlayback.PlaybackDevice.PlaybackState == PlaybackState.Stopped;

        public WaveStream FileStream { get => audioPlayback.FileStream; }

        public PlotModel PlotModel
        {
            get => plotModel;
            set
            {
                plotModel = value;
                OnPropertyChanged("PlotModel");
            }
        }

        public FftSharp.IWindow WindowFunc
        {
            get; set;
        }

        public float SingleFrequency
        {
            get => singleFrequency;
            set
            {
                if (singleFrequency != value)
                {
                    singleFrequency = value;
                    OnPropertyChanged("SingleFrequency");
                }
            }
        }
        public string SingleNote
        {
            get => singleNote;
            set
            {
                if (singleNote != value)
                {
                    singleNote = value;
                    OnPropertyChanged("SingleNote");
                }
            }
        }

        public void Play()
        {

            if (audioPlayback.PlaybackDevice == null)
            {
                CreatePlayer();
            }
            if (lastPlayed != InputPath && audioPlayback.FileStream != null)
            {
                audioPlayback.CloseFile();
            }
            if (audioPlayback.FileStream == null)
            {
                audioPlayback.Load(InputPath);
                lastPlayed = InputPath;
                AudioValues = new double[audioPlayback.SampleRate / 10];
                chromagram.Initialize(audioPlayback.SampleRate);
            }
            audioPlayback.Play();
            timer.Start();
        }

        private void CreatePlayer()
        {
            audioPlayback.CreateDevice();
            audioPlayback.PlaybackDevice.PlaybackStopped += PlaybackStopped;
        }

        public void Stop()
        {
            if (audioPlayback.PlaybackDevice != null)
            {
                audioPlayback.Stop();
                timer.Stop();
            }
        }

        public void Pause()
        {
            if (audioPlayback.PlaybackDevice != null)
            {
                audioPlayback.Pause();
                timer.Stop();
            }
        }

        public void Load()
        {
            if (audioPlayback.FileStream != null)
            {
                audioPlayback.CloseFile();
                plotModel.Series.Clear();
                plotModel.Series.Add(new LineSeries());
                plotModel.InvalidatePlot(true);
            }
        }

        public void Dispose()
        {
            audioPlayback.Dispose();
        }
    }
}
