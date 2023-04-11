using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using System;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PitchFinder.Models
{
    class FileAudioHandler : AudioHandler, IAudioWrapper
    {
        private IWavePlayer _playbackWave;
        private string _inputPath;
        private string _lastPlayed;

        public WaveStreamWrapper WaveWrapper { get; set; }
        public override bool IsPlaying => _playbackWave != null && _playbackWave.PlaybackState == PlaybackState.Playing;
        public override bool IsStopped => _playbackWave == null || _playbackWave.PlaybackState == PlaybackState.Stopped;

        public override event EventHandler<StoppedEventArgs> PlaybackStopped;

        void audioGraph_Buffer(object sender, BufferEventArgs e)
        {
            ReadSamples(e.Buffer);
        }

        private void Load(string fileName)
        {
            Stop();
            CloseFile();
            EnsureDeviceCreated();
            OpenFile(fileName);
        }

        private void CloseFile()
        {
            WaveWrapper?.Dispose();
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                WaveWrapper.Init(inputStream);
                _reader = new SampleReader(inputStream.WaveFormat);
                var aggregator = new SampleAggregator(WaveWrapper.SoundTouchProvider);
                SampleRate = inputStream.WaveFormat.SampleRate;
                aggregator.BufferEventArgs += audioGraph_Buffer;
                _playbackWave.Init(aggregator);
            }
            catch (Exception e)
            {
                CloseFile();
                throw new Exception(e.Message);
            }
        }

        private void EnsureDeviceCreated()
        {
            if (_playbackWave == null)
                CreateDevice();
        }

        private void CreateDevice()
        {
            _playbackWave = new WaveOutEvent { DesiredLatency = 100 };
        }

        public override void Play()
        {
            if (string.IsNullOrEmpty(_inputPath))
            {
                throw new Exception("Select a valid input file or URL first");
            }

            if (_playbackWave == null)
            {
                CreatePlayer();
            }
            if (_lastPlayed != _inputPath && WaveWrapper.WaveStream != null)
            {
                CloseFile();
            }
            if (WaveWrapper.WaveStream == null)
            {
                Load(_inputPath);
                _lastPlayed = _inputPath;
                _audioValues = new double[4096];
                _inputBack = new double[4096];
                WeakReferenceMessenger.Default.Send(new Messages.SampleRateChangedMessage(SampleRate));
            }

            _playbackWave.Play();
        }

        private void CreatePlayer()
        {
            CreateDevice();
            _playbackWave.PlaybackStopped += PlaybackStopped;
        }

        public override void Stop()
        {
            if (_playbackWave != null)
            {
                _playbackWave?.Stop();
                if (WaveWrapper.WaveStream != null)
                    WaveWrapper.WaveStream.Position = 0;
            }
        }

        public override void Pause()
        {
            if (_playbackWave != null)
                _playbackWave.Pause();
        }

        public override bool Load()
        {
            if (!SelectInputFile())
                return false;

            if (WaveWrapper.WaveStream != null)
                CloseFile();
            else
                base.Load();

            return true;
        }

        private bool SelectInputFile()
        {
            var ofd = new OpenFileDialog();

            bool result = (bool)ofd.ShowDialog();
            if (result == true)
                OpenInputFile(ofd.FileName);

            return result;
        }

        private void OpenInputFile(string file)
        {
            try
            {
                using (var tempReader = new AudioFileReader(file))
                {
                    _inputPath = file;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Not a supported input file ({e.Message})");
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Stop();
            CloseFile();
            _playbackWave?.Dispose();
            _playbackWave = null;
            WaveWrapper = null;
        }
    }
}
