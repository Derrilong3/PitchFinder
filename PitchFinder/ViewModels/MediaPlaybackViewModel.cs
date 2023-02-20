using NAudio.Wave;
using PitchFinder.Models;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace PitchFinder.ViewModels
{
    internal class MediaPlaybackViewModel : ToolViewModel, IDisposable
    {
        const double SliderMax = 10.0;
        private string timerPosition;
        private double sliderPosition;

        private readonly DispatcherTimer timer = new DispatcherTimer();
        private IAudioHandler _audioHandler;
        private AudioAnalyzeModel _analyzeModel;

        public bool IsPlaying { get => _audioHandler.IsPlaying; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand PlayPauseCommand { get; }
        public RelayCommand StopCommand { get; }

        public MediaPlaybackViewModel() : base("Media Window")
        {
            ContentId = "MediaTool";

            Init(typeof(FileAudioHandler));

            LoadCommand = new RelayCommand(obj => Load((Type)obj), (obj) => _audioHandler.IsStopped);
            PlayPauseCommand = new RelayCommand(PlayPauseInvoke);
            StopCommand = new RelayCommand(Stop, () => !_audioHandler.IsStopped);

            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += TimerOnTick;

            TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
        }

        private void WavePlayerOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            SliderPosition = 0;
            TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
            timer.Stop();

            if (stoppedEventArgs.Exception != null)
            {
                MessageBox.Show(stoppedEventArgs.Exception.Message, "Error Playing File");
            }
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            sliderPosition = Math.Min(SliderMax, _audioHandler.Position * SliderMax / _audioHandler.Length);
            TimePosition = _audioHandler.CurrentTime.ToString("mm\\:ss");
            OnPropertyChanged("SliderPosition");
        }

        public string TimePosition
        {
            get => timerPosition;
            set
            {
                if (timerPosition == value)
                    return;

                timerPosition = value;
                OnPropertyChanged("TimePosition");
            }
        }

        public double SliderPosition
        {
            get => sliderPosition;
            set
            {
                if (sliderPosition != value)
                {
                    sliderPosition = value;
                    var pos = (long)(_audioHandler.Length * sliderPosition / SliderMax);
                    _audioHandler.Position = pos;
                    OnPropertyChanged("SliderPosition");
                }
            }
        }

        private void PlayPauseInvoke()
        {
            if (IsPlaying)
                Pause();
            else
                Play();

            OnPropertyChanged("IsPlaying");
        }

        private void Play()
        {
            try
            {
                _audioHandler.Play();
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Stop()
        {
            _audioHandler.Stop();
            OnPropertyChanged("IsPlaying");
        }

        private void Pause()
        {
            _audioHandler.Pause();
            timer.Stop();
        }

        private void Load(Type audioHandler)
        {
            try
            {
                if(_audioHandler?.GetType() != audioHandler)
                {
                    Init(audioHandler);
                }

                _audioHandler.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Init(Type type)
        {
            _audioHandler?.Dispose();
            _audioHandler = (IAudioHandler)Activator.CreateInstance(type);
            _audioHandler.PlaybackStopped += WavePlayerOnPlaybackStopped;
            _analyzeModel = new AudioAnalyzeModel(_audioHandler);
        }

        public void Dispose()
        {
            _audioHandler.Dispose();
        }

    }
}
