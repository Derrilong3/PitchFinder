using NAudio.Wave;
using PitchFinder.Models;
using System;
using System.Windows;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PitchFinder.ViewModels
{
    internal class MediaPlaybackViewModel : ToolViewModel, IDisposable
    {
        const double SliderMax = 10.0;
        private string timerPosition;
        private string defaultDecompressionFormat;
        private double sliderPosition;

        private readonly DispatcherTimer timer = new DispatcherTimer();
        private IAudioHandler _audioHandler;
        private AudioAnalyzeModel _analyzeModel;

        public RelayCommand LoadCommand { get; }
        public RelayCommand PlayPauseCommand { get; }
        public RelayCommand StopCommand { get; }

        public MediaPlaybackViewModel() : base("Media Window")
        {
            ContentId = "MediaTool";
            _audioHandler = new AudioFileHandler();
            _audioHandler.PlaybackStopped += WavePlayerOnPlaybackStopped;
            _analyzeModel = new AudioAnalyzeModel(_audioHandler);
            LoadCommand = new RelayCommand(Load, () => _audioHandler.IsStopped);
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

        public bool IsPlaying
        {
            get => _audioHandler.IsPlaying;
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

        public string DefaultDecompressionFormat
        {
            get => defaultDecompressionFormat;
            set
            {
                defaultDecompressionFormat = value;
                OnPropertyChanged("DefaultDecompressionFormat");
            }
        }

        public string InputPath
        {
            get => _audioHandler.InputPath;
            set
            {
                if (_audioHandler.InputPath != value)
                {
                    _audioHandler.InputPath = value;
                    OnPropertyChanged("InputPath");
                }
            }
        }

        private void SelectInputFile()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                OpenInputFile(ofd.FileName);
            }
        }

        private void OpenInputFile(string file)
        {
            try
            {
                using (var tempReader = new AudioFileReader(file))
                {
                    DefaultDecompressionFormat = tempReader.WaveFormat.ToString();
                    InputPath = file;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Not a supported input file ({e.Message})");
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
            if (string.IsNullOrEmpty(InputPath))
            {
                MessageBox.Show("Select a valid input file or URL first");
                return;
            }

            _audioHandler.Play();
            timer.Start();
        }

        private void Stop()
        {
            _audioHandler.Stop();
        }

        private void Pause()
        {
            _audioHandler.Pause();
            timer.Stop();
        }

        private void Load()
        {
            _audioHandler.Load();
            SelectInputFile();
        }

        public void Dispose()
        {
            _audioHandler.Dispose();
        }

    }
}
