using CommunityToolkit.Mvvm.Messaging;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PitchFinder.Controls;
using System;
using System.Linq;

namespace PitchFinder.Models
{
    internal class DeviceAudioHandler : AudioHandler
    {
        private WasapiCapture _capture;
        private WaveFormat _waveFormat;
        private MMDevice? _device;

        public override bool IsPlaying => _capture != null && (_capture.CaptureState == CaptureState.Starting ^ _capture.CaptureState == CaptureState.Capturing);
        public override bool IsStopped => _capture == null || _capture.CaptureState == CaptureState.Stopped || _capture.CaptureState == CaptureState.Stopping;

        public override event EventHandler<StoppedEventArgs> PlaybackStopped;

        private void DataAvailable(object? sender, WaveInEventArgs e)
        {
            ReadSamples(e.Buffer);
        }

        public override void Play()
        {
            _capture?.StartRecording();
        }

        public override void Pause()
        {
            _capture.StopRecording();
        }

        public override void Stop()
        {
            _capture.StopRecording();
            PlaybackStopped.Invoke(this, new StoppedEventArgs());
        }

        public override bool Load()
        {
            if (!GetDevice())
                return false;


            if (_capture == null)
                base.Load();

            InitDeviceCapture();

            return true;
        }

        private void InitDeviceCapture()
        {
            _waveFormat = _device.AudioClient.MixFormat.AsStandardWaveFormat();

            _audioValues = new double[4096];
            _inputBack = new double[4096];

            SampleRate = _waveFormat.SampleRate;
            WeakReferenceMessenger.Default.Send(new Messages.SampleRateChangedMessage(SampleRate));

            WasapiCapture capture;
            if (_device.DataFlow == DataFlow.Render)
                capture = new WasapiLoopbackCapture(_device);
            else
                capture = new WasapiCapture(_device, false, 100) { WaveFormat = _waveFormat };

            capture.DataAvailable += DataAvailable;

            _capture = capture;

            _reader = new SampleReader(_waveFormat);
        }

        private bool GetDevice()
        {
            MMDevice[] audioDevices = new MMDeviceEnumerator()
           .EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active)
           .ToArray();

            string[] names = audioDevices.Select(x => x.FriendlyName).ToArray();

            ListBoxDialog dialog = new ListBoxDialog("Chose input device", names);
            if (dialog.ShowDialog() == true)
            {
                _device = audioDevices[dialog.SelectedIndex];
                return true;
            }

            return false;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_capture?.CaptureState == CaptureState.Stopped)
            {
                _capture.Dispose();
            }
            else
            {
                _capture?.Dispose();
                _device?.Dispose();
            }
        }
    }
}
