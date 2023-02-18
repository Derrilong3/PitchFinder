using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PitchFinder.Models
{
    internal abstract class AudioHandler : IAudioHandler
    {
        protected double[] _audioValues;
        protected double[] _inputBack;
        protected string _lastPlayed;

        protected CancellationTokenSource _cts;
        protected CancellationToken _token;
        protected AutoResetEvent _processEvt;
        protected SampleReader _reader;

        public double[] Samples => _inputBack;
        public int SampleRate { get; set; }

        public virtual bool IsPlaying { get; }
        public virtual bool IsStopped { get; }

        public string InputPath { get; set; }

        public virtual long Position { get; set; }
        public virtual long Length { get; }
        public virtual TimeSpan CurrentTime { get; }

        public virtual event EventHandler<StoppedEventArgs> PlaybackStopped;
        public event EventHandler DataReceived;

        public AudioHandler()
        {
            _cts = new CancellationTokenSource();
            _token = _cts.Token;

            _processEvt = new AutoResetEvent(false);
            _ = Task.Run(ProcessData, _cts.Token);
        }

        private void ProcessData()
        {
            while (!_token.IsCancellationRequested)
            {
                if (_processEvt.WaitOne(100))
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

        public virtual void Load()
        {
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
        }
    }
}
