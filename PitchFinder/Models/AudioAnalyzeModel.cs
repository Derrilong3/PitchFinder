using CommunityToolkit.Mvvm.Messaging;
using FftSharp;
using NAudio.Wave;
using PitchFinder.ViewModels;
using System;
using System.Timers;

namespace PitchFinder.Models
{
    internal class AudioAnalyzeModel : ViewModelBase, IDisposable
    {
        private string _lastPlayed;
        private double[] _audioValues;
        private readonly Timer _timer;
        private bool _timered;
        private FftSharp.IWindow _windowFunc;

        public IWavePlayer PlaybackDevice { get; set; }
        public WaveStream FileStream { get; set; }
        public int SampleRate { get; set; }
        public string InputPath { get; set; }
        public bool IsPlaying => PlaybackDevice != null && PlaybackDevice.PlaybackState == PlaybackState.Playing;
        public bool IsStopped => PlaybackDevice == null || PlaybackDevice.PlaybackState == PlaybackState.Stopped;

        public AudioAnalyzeModel()
        {
            _windowFunc = Window.GetWindows()[Properties.Settings.Default.selectedFunc];

            _timer = new Timer(50);
            _timer.Elapsed += TimerOnElapsed;
            _timer.AutoReset = true;

            WeakReferenceMessenger.Default.Register<Messages.WindowFuncChangedMessage>(this, WindowChanged);
        }

        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        public void WindowChanged(object obj, Messages.WindowFuncChangedMessage message)
        {
            _windowFunc = Window.GetWindows()[message.Value];
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_timered)
            {
                return;
            }

            double[] windowed = _windowFunc.Apply(_audioValues);
            double[] paddedAudio = FftSharp.Pad.ZeroPad(windowed);

            Messages.FFTData fft = new Messages.FFTData();
            fft.Y = FftSharp.Transform.FFTmagnitude(paddedAudio);
            fft.X = FftSharp.Transform.FFTfreq(SampleRate, fft.Y.Length);

            WeakReferenceMessenger.Default.Send(new Messages.FFTChangedMessage(fft));
            _timered = false;
        }

        void audioGraph_Buffer(object sender, BufferEventArgs e)
        {
            int bytesPerSamplePerChannel = FileStream.WaveFormat.BitsPerSample / 8;
            int bytesPerSample = bytesPerSamplePerChannel * FileStream.WaveFormat.Channels;
            int bufferSampleCount = e.Buffer.Length / bytesPerSample;

            if (bufferSampleCount >= _audioValues.Length)
            {
                bufferSampleCount = _audioValues.Length;
            }

            if (bytesPerSamplePerChannel == 2 && FileStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    _audioValues[i] = BitConverter.ToInt16(e.Buffer, i * bytesPerSample);
            }
            else if (bytesPerSamplePerChannel == 4 && FileStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    _audioValues[i] = BitConverter.ToInt32(e.Buffer, i * bytesPerSample);
            }
            else if (bytesPerSamplePerChannel == 4 && FileStream.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    _audioValues[i] = BitConverter.ToSingle(e.Buffer, i * bytesPerSample);
            }
            else
            {
                throw new NotSupportedException(FileStream.WaveFormat.ToString());
            }

            _timered = true;
        }

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
                aggregator.BufferEventArgs += audioGraph_Buffer;
                PlaybackDevice.Init(aggregator);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Problem opening file");
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
            PlaybackDevice = new WaveOut { DesiredLatency = 150 };
        }

        public void Play()
        {
            if (PlaybackDevice == null)
            {
                CreatePlayer();
            }
            if (_lastPlayed != InputPath && FileStream != null)
            {
                CloseFile();
            }
            if (FileStream == null)
            {
                Load(InputPath);
                _lastPlayed = InputPath;
                _audioValues = new double[SampleRate / 10];
                WeakReferenceMessenger.Default.Send(new Messages.SampleRateChangedMessage(SampleRate));
            }

            PlaybackDevice.Play();
            _timer.Start();
        }

        private void CreatePlayer()
        {
            CreateDevice();
            PlaybackDevice.PlaybackStopped += PlaybackStopped;
        }

        public void Stop()
        {
            if (PlaybackDevice != null)
            {
                PlaybackDevice?.Stop();
                if (FileStream != null)
                {
                    FileStream.Position = 0;
                }
                _timer.Stop();
            }
        }

        public void Pause()
        {
            if (PlaybackDevice != null)
            {
                PlaybackDevice.Pause();
                _timer.Stop();
            }
        }

        public void Load()
        {
            if (FileStream != null)
            {
                CloseFile();
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
