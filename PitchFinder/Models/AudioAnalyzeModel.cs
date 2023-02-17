using CommunityToolkit.Mvvm.Messaging;
using FftSharp;
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
            double[] windowed = _windowFunc.Apply(_handler.Samples);
            double[] paddedAudio = FftSharp.Pad.ZeroPad(windowed);

            Messages.FFTData fft = new Messages.FFTData();
            fft.Y = FftSharp.Transform.FFTmagnitude(paddedAudio);
            fft.X = FftSharp.Transform.FFTfreq(_handler.SampleRate, fft.Y.Length);

            WeakReferenceMessenger.Default.Send(new Messages.FFTChangedMessage(fft));
        }
    }
}
