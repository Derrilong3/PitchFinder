using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PitchFinder.Models
{
    public static class Messages
    {
        public class FFTData
        {
            public double[] X { get; set; }
            public double[] Y { get; set; }
        }

        public class FFTChangedMessage : ValueChangedMessage<FFTData>
        {
            public FFTChangedMessage(FFTData fft) : base(fft)
            {
            }
        }

        public class SampleRateChangedMessage : ValueChangedMessage<int>
        {
            public SampleRateChangedMessage(int sampleRate) : base(sampleRate)
            {
            }
        }

        public class WindowFuncChangedMessage : ValueChangedMessage<int>
        {
            public WindowFuncChangedMessage(int windowIndex) : base(windowIndex)
            {
            }
        }
    }
}
