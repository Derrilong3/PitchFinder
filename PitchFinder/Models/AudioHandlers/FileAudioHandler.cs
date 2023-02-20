using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using System;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PitchFinder.Models
{
    class FileAudioHandler : AudioHandler
    {
        private IWavePlayer _playbackWave;
        private WaveStream _fileStream;
        private string _inputPath;

        public override bool IsPlaying => _playbackWave != null && _playbackWave.PlaybackState == PlaybackState.Playing;
        public override bool IsStopped => _playbackWave == null || _playbackWave.PlaybackState == PlaybackState.Stopped;

        public override long Position
        {
            get => _fileStream.Position;
            set
            {
                if (_fileStream != null)
                    _fileStream.Position = value;
            }
        }

        public override long Length => _fileStream != null ? _fileStream.Length : 0;
        public override TimeSpan CurrentTime => _fileStream.CurrentTime;

        public override event EventHandler<StoppedEventArgs> PlaybackStopped;

        void audioGraph_Buffer(object sender, BufferEventArgs e)
        {
            ReadSamples(e.Buffer);
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
            _fileStream?.Dispose();
            _fileStream = null;
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                _fileStream = inputStream;
                _reader = new SampleReader(inputStream.WaveFormat);
                var aggregator = new SampleAggregator(inputStream);
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
            {
                CreateDevice();
            }
        }

        public void CreateDevice()
        {
            _playbackWave = new WaveOut { DesiredLatency = 100 };
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
            if (_lastPlayed != _inputPath && _fileStream != null)
            {
                CloseFile();
            }
            if (_fileStream == null)
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
                if (_fileStream != null)
                {
                    _fileStream.Position = 0;
                }
            }
        }

        public override void Pause()
        {
            if (_playbackWave != null)
            {
                _playbackWave.Pause();
            }
        }

        public override void Load()
        {
            if (_fileStream != null)
            {
                CloseFile();
            }

            SelectInputFile();
        }

        private void SelectInputFile()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                OpenInputFile(ofd.FileName);
            }
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
        }
    }
}
