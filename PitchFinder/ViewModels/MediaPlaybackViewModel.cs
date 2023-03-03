using NAudio.Wave;
using PitchFinder.Models;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PitchFinder.ViewModels
{
    internal class MediaPlaybackViewModel : ViewModelBase, IDisposable
    {
        private const double SliderMax = 10.0;

        private DispatcherTimer _timer;
        private IAudioHandler _audioHandler;
        private AudioAnalyzeModel _analyzeModel;
        private WaveStreamWrapper _waveStream;

        public bool IsPlaying { get => _audioHandler.IsPlaying; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand PlayPauseCommand { get; }
        public RelayCommand StopCommand { get; }

        public MediaPlaybackViewModel()
        {
            _waveStream = new WaveStreamWrapper();

            Init(typeof(FileAudioHandler));

            LoadCommand = new RelayCommand(obj => Load((Type)obj), (obj) => _audioHandler.IsStopped);
            PlayPauseCommand = new RelayCommand(PlayPauseInvoke);
            StopCommand = new RelayCommand(Stop, () => !_audioHandler.IsStopped);
            Tempo = 1;

            TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
        }

        private void WavePlayerOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            SliderPosition = 0;
            TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
            _timer.Stop();
            CommandManager.InvalidateRequerySuggested();
            OnPropertyChanged("IsPlaying");

            if (stoppedEventArgs.Exception != null)
            {
                MessageBox.Show(stoppedEventArgs.Exception.Message, "Error Playing File");
            }
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            _sliderPosition = Math.Min(SliderMax, _waveStream.WaveStream.Position * SliderMax / _waveStream.WaveStream.Length);
            TimePosition = _waveStream.WaveStream.CurrentTime.ToString("mm\\:ss");
            OnPropertyChanged("SliderPosition");
        }

        private string _timerPosition;
        public string TimePosition
        {
            get => _timerPosition;
            set
            {
                if (_timerPosition == value)
                    return;

                _timerPosition = value;
                OnPropertyChanged("TimePosition");
            }
        }

        private double _sliderPosition;
        public double SliderPosition
        {
            get => _sliderPosition;
            set
            {
                if (_sliderPosition != value)
                {
                    _sliderPosition = value;
                    if (_waveStream.WaveStream != null)
                    {
                        var pos = (long)(_waveStream.WaveStream.Length * _sliderPosition / SliderMax);
                        _waveStream.WaveStream.Position = pos;
                        OnPropertyChanged("SliderPosition");
                    }
                }
            }
        }

        public double Tempo
        {
            get => _waveStream.Tempo;
            set
            {
                _waveStream.Tempo = value;
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
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Stop()
        {
            _audioHandler.Stop();
        }

        private void Pause()
        {
            _audioHandler.Pause();
            _timer.Stop();
        }

        private void Load(Type audioHandler)
        {
            try
            {
                if (_audioHandler?.GetType() != audioHandler)
                {
                    Init(audioHandler);
                }

                if (_audioHandler.Load())
                {
                    PlayPauseInvoke();
                }
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
            _timer = new DispatcherTimer();

            if (type.GetInterface(nameof(IAudioProgressBar)) != null)
            {
                ((IAudioProgressBar)_audioHandler).WaveWrapper = _waveStream;
                _timer.Interval = TimeSpan.FromMilliseconds(10);
                _timer.Tick += TimerOnTick;
            }
        }

        public void Dispose()
        {
            _audioHandler.Dispose();
        }
    }
}
