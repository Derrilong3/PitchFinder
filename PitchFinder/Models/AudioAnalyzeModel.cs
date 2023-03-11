using CommunityToolkit.Mvvm.Messaging;
using FftSharp;
using NAudio.Dsp;
using PitchFinder.ViewModels;
using System;

namespace PitchFinder.Models
{
    internal class AudioAnalyzeModel : ViewModelBase
    {
        private FftSharp.IWindow _windowFunc;
        private readonly IAudioHandler _handler;

        public AudioAnalyzeModel(IAudioHandler handler)
        {
            _handler = handler;
            _windowFunc = Window.GetWindows()[Properties.Settings.Default.selectedFunc];

            handler.DataReceived += Handler_DataReceived;
            WeakReferenceMessenger.Default.Register<Messages.WindowFuncChangedMessage>(this, WindowChanged);
        }

        public void WindowChanged(object obj, Messages.WindowFuncChangedMessage message)
        {
            _windowFunc = Window.GetWindows()[message.Value];
        }

        private void Handler_DataReceived(object sender, EventArgs e)
        {
            double[] win = _windowFunc.Create(_handler.Samples.Length);
            Messages.FFTData fft = new Messages.FFTData();
            NAudio.Dsp.Complex[] complex = new NAudio.Dsp.Complex[win.Length];
            for (int i = 0; i < win.Length; i++)
            {
                complex[i].X = (float)(_handler.Samples[i] * win[i]);
                complex[i].Y = 0;
            }

            FastFourierTransform.FFT(true, (int)Math.Log(complex.Length, 2.0), complex);
            double[] temp = new double[win.Length / 2];
            for (int i = 0; i < temp.Length; i++)
            {
                double mag = Math.Sqrt(complex[i].X * complex[i].X + complex[i].Y * complex[i].Y);
                temp[i] = mag;
            }

            fft.Y = temp;

            double[] freqs = new double[fft.Y.Length];


            double fftPeriodHz = (double)_handler.SampleRate / (fft.Y.Length) / 2;

            // freqs start at 0 and approach maxFreq
            for (int i = 0; i < fft.Y.Length; i++)
                freqs[i] = i * fftPeriodHz;

            fft.X = freqs;

            WeakReferenceMessenger.Default.Send(new Messages.FFTChangedMessage(fft));
        }
    }
}
