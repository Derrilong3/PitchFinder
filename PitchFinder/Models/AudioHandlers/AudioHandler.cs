using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PitchFinder.Models
{
    internal abstract class AudioHandler : IAudioHandler
    {
        protected double[] _audioValues;
        protected double[] _inputBack;
        protected string _lastPlayed;

        private CancellationTokenSource _cts;
        private CancellationToken _token;
        private AutoResetEvent _processEvt;
        protected SampleReader _reader;

        public double[] Samples => _inputBack;
        public int SampleRate { get; set; }

        public virtual bool IsPlaying { get; }
        public virtual bool IsStopped { get; }

        public virtual long Position { get; set; }
        public virtual long Length { get; }
        public virtual TimeSpan CurrentTime { get; }

        public virtual event EventHandler<StoppedEventArgs> PlaybackStopped;
        public event EventHandler DataReceived;

        private void ProcessData()
        {
            while (!_token.IsCancellationRequested)
            {
                if (_processEvt.WaitOne())
                {
                    lock (_audioValues)
                    {
                        Array.Copy(_audioValues, _inputBack, _audioValues.Length);
                    }
                    DataReceived?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected void ReadSamples(byte[] buffer)
        {
            lock (_audioValues)
            {
                _reader.ReadSamples(buffer, buffer.Length, _audioValues);
                _processEvt.Set();
            }
        }

        public virtual bool Load()
        {
            _cts = new CancellationTokenSource();
            _token = _cts.Token;

            _processEvt = new AutoResetEvent(false);
            _ = Task.Run(ProcessData, _cts.Token);

            return true;
        }

        public virtual void Pause()
        {
        }

        public virtual void Play()
        {
        }

        public virtual void Stop()
        {
        }

        public virtual void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
