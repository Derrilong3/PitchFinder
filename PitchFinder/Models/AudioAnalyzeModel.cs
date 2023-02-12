using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using PitchFinder.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Timers;

namespace PitchFinder.Models
{
    internal class AudioAnalyzeModel : ViewModelBase, IDisposable
    {
        private string lastPlayed;
        private double[] AudioValues;
        private AudioPlayback audioPlayback;     
        private readonly System.Timers.Timer timer;
        private bool timered;

        public ObservableCollection<FftSharp.IWindow> WindowFunctions { get; private set; }

        public AudioAnalyzeModel()
        {
            audioPlayback = new AudioPlayback();
            audioPlayback.BufferEventArgs += audioGraph_Buffer;

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

            double[] windowed = WindowFunc.Apply(AudioValues);
            double[] paddedAudio = FftSharp.Pad.ZeroPad(windowed);

            Messages.FFTData fft = new Messages.FFTData();
            fft.Y = FftSharp.Transform.FFTmagnitude(paddedAudio);
            fft.X = FftSharp.Transform.FFTfreq(audioPlayback.SampleRate, fft.Y.Length);

            WeakReferenceMessenger.Default.Send(new Messages.FFTChangedMessage(fft));
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

        public FftSharp.IWindow WindowFunc
        {
            get; set;
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
                WeakReferenceMessenger.Default.Send(new Messages.SampleRateChangedMessage(audioPlayback.SampleRate));
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
            }
        }

        public void Dispose()
        {
            audioPlayback.Dispose();
        }
    }
}
