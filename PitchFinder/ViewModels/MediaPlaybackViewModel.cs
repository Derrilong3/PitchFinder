using NAudio.Wave;
using OxyPlot;
using PitchFinder.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PitchFinder.ViewModels
{
    internal class MediaPlaybackViewModel : ToolViewModel, IDisposable
    {
        private readonly ObservableCollection<string> inputPathHistory;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        const double SliderMax = 10.0;
        private string timerPosition;
        private string defaultDecompressionFormat;
        private double sliderPosition;
        private AudioAnalyzeModel model;

        public RelayCommand LoadCommand { get; }
        public RelayCommand PlayCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand StopCommand { get; }

        public MediaPlaybackViewModel() : base("Media Window")
        {
            ContentId = "MediaTool";
            model = new AudioAnalyzeModel();
            model.PlaybackStopped += WavePlayerOnPlaybackStopped;
            model.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            inputPathHistory = new ObservableCollection<string>();
            LoadCommand = new RelayCommand(Load, () => model.IsStopped);
            PlayCommand = new RelayCommand(Play, () => !model.IsPlaying);
            PauseCommand = new RelayCommand(Pause, () => model.IsPlaying);
            StopCommand = new RelayCommand(Stop, () => !model.IsStopped);
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += TimerOnTick;
            TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
        }

        private void WavePlayerOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (model.FileStream != null)
            {
                SliderPosition = 0;
                TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
                //reader.Position = 0;
                timer.Stop();
            }
            if (stoppedEventArgs.Exception != null)
            {
                MessageBox.Show(stoppedEventArgs.Exception.Message, "Error Playing File");
            }
            //OnPropertyChanged("IsPlaying");
            //OnPropertyChanged("IsStopped");
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (model.FileStream != null)
            {
                sliderPosition = Math.Min(SliderMax, model.FileStream.Position * SliderMax / model.FileStream.Length);
                TimePosition = model.FileStream.CurrentTime.ToString("mm\\:ss");
                OnPropertyChanged("SliderPosition");
            }
        }

        public PlotModel PlotModel
        {
            get => model.PlotModel;
        }

        public ObservableCollection<NoteBox> ColorMulti
        {
            get => model.ColorMulti;
        }

        public ObservableCollection<FftSharp.IWindow> WindowFunctions
        {
            get => model.WindowFunctions;
        }

        public FftSharp.IWindow SelectedFunc
        {
            get
            {
                return model.WindowFunc;
            }
            set 
            {
                model.WindowFunc = value; 
                OnPropertyChanged("SelectedFunc"); 
            }
        }

        public float SingleFrequency
        {
            get => model.SingleFrequency;
        }

        public string SingleNote
        {
            get => model.SingleNote;
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
                    if (model.FileStream != null)
                    {
                        var pos = (long)(model.FileStream.Length * sliderPosition / SliderMax);
                        model.FileStream.Position = pos;
                    }
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
            get => model.InputPath;
            set
            {
                if (model.InputPath != value)
                {
                    model.InputPath = value;
                    AddToHistory(value);
                    OnPropertyChanged("InputPath");
                }
            }
        }

        private void AddToHistory(string value)
        {
            if (!inputPathHistory.Contains(value))
            {
                inputPathHistory.Add(value);
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

        private void Play()
        {
            if (string.IsNullOrEmpty(InputPath))
            {
                MessageBox.Show("Select a valid input file or URL first");
                return;
            }

            model.Play();
            timer.Start();
        }

        private void Stop()
        {
            model.Stop();
        }

        private void Pause()
        {
            model.Pause();
            timer.Stop();
        }

        private void Load()
        {
            model.Load();
            SelectInputFile();
        }

        public void Dispose()
        {
            model.Dispose();
        }

    }
}
