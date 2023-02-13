using NAudio.Wave;
using System;
using System.Diagnostics;

namespace PitchFinder.Models
{
    public class SampleAggregator : IWaveProvider
    {
        public event EventHandler<BufferEventArgs> BufferEventArgs;
        private readonly IWaveProvider source;

        public SampleAggregator(IWaveProvider source)
        {
            this.source = source;
        }

        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(byte[] buffer, int offset, int count)
        {
            var samplesRead = source.Read(buffer, offset, count);
            BufferEventArgs.Invoke(this, new BufferEventArgs(buffer));
            return samplesRead;
        }
    }

    public class BufferEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public BufferEventArgs(byte[] result)
        {
            Buffer = result;
        }
        public byte[] Buffer { get; private set; }
    }
}