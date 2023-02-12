using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            public SampleRateChangedMessage(in int sampleRate) : base(sampleRate)
            {
            }
        }
    }
}
