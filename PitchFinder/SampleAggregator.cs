using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Diagnostics;

public class SampleAggregator : IWaveProvider
{
    // volume
    public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
    private float maxValue;
    private float minValue;
    public int NotificationCount { get; set; }
    int count;

    // FFT
    public event EventHandler<FftEventArgs> FftCalculated;
    public event EventHandler<BufferEventArgs> BufferEventArgs;
    public bool PerformFFT { get; set; }
    private readonly System.Numerics.Complex[] fftBuffer;
    private readonly FftEventArgs fftArgs;
    private int fftPos;
    private readonly int fftLength;
    private readonly int m;
    private readonly IWaveProvider source;
    
    private readonly int channels;

    public SampleAggregator(IWaveProvider source, int fftLength = 4096)
    {
        
        channels = source.WaveFormat.Channels;
        if (!IsPowerOfTwo(fftLength))
        {
            throw new ArgumentException("FFT Length must be a power of two");
        }
        m = (int)Math.Log(fftLength, 2.0);
        this.fftLength = fftLength;
        fftBuffer = new System.Numerics.Complex[fftLength];
        fftArgs = new FftEventArgs(fftBuffer);
        this.source = source;
    }

    static bool IsPowerOfTwo(int x)
    {
        return (x & (x - 1)) == 0;
    }


    public void Reset()
    {
        count = 0;
        maxValue = minValue = 0;
    }

    private void Add(float value)
    {
        if (PerformFFT && FftCalculated != null)
        {
            fftBuffer[fftPos] = new System.Numerics.Complex(value * FastFourierTransform.HannWindow(fftPos, fftLength), 0);
            //fftBuffer[fftPos].Y = 0;
            fftPos++;
            if (fftPos >= fftBuffer.Length)
            {
                fftPos = 0;
                // 1024 = 2^10
                Accord.Math.FourierTransform.FFT(fftBuffer, Accord.Math.FourierTransform.Direction.Forward);
                //FastFourierTransform.FFT(true, m, fftBuffer);
                FftCalculated(this, fftArgs);
            }
        }

        maxValue = Math.Max(maxValue, value);
        minValue = Math.Min(minValue, value);
        count++;
        if (count >= NotificationCount && NotificationCount > 0)
        {
            MaximumCalculated?.Invoke(this, new MaxSampleEventArgs(minValue, maxValue));
            Reset();
        }
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(byte[] buffer, int offset, int count)
    {
        var samplesRead = source.Read(buffer, offset, count);
        BufferEventArgs.Invoke(this, new BufferEventArgs(buffer));
        //for (int n = 0; n < samplesRead; n += channels)
        //{
        //    Add(buffer[n + offset]);
        //}
        return samplesRead;
    }
}

public class MaxSampleEventArgs : EventArgs
{
    [DebuggerStepThrough]
    public MaxSampleEventArgs(float minValue, float maxValue)
    {
        MaxSample = maxValue;
        MinSample = minValue;
    }
    public float MaxSample { get; private set; }
    public float MinSample { get; private set; }
}

public class FftEventArgs : EventArgs
{
    [DebuggerStepThrough]
    public FftEventArgs(System.Numerics.Complex[] result)
    {
        Result = result;
    }
    public System.Numerics.Complex[] Result { get; private set; }
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