using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using System;

namespace PitchFinder.Models
{
    class AudioFileHandler : AudioHandler
    {
        private IWavePlayer _playbackWave;
        private WaveStream _fileStream;

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
            lock (_audioValues)
            {
                _reader.ReadSamples(e.Buffer, e.Buffer.Length, _audioValues);
                _processEvt.Set();
            }
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
                System.Windows.MessageBox.Show(e.Message, "Problem opening file");
                CloseFile();
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
            if (_playbackWave == null)
            {
                CreatePlayer();
            }
            if (_lastPlayed != InputPath && _fileStream != null)
            {
                CloseFile();
            }
            if (_fileStream == null)
            {
                Load(InputPath);
                _lastPlayed = InputPath;
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
        }

        public override void Dispose()
        {
            Stop();
            CloseFile();
            _playbackWave?.Dispose();
            _playbackWave = null;
        }
    }
}
