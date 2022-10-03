using NAudio.Wave;
using PitchFinder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PitchFinder
{
    internal class MediaFoundationPlaybackViewModel : ViewModelBase, IDisposable
    {
        private int requestFloatOutput;
        private string inputPath;
        private string defaultDecompressionFormat;
        //private IWavePlayer wavePlayer;
        //private WaveStream reader;
        private AudioPlayback audioPlayback;
        public RelayCommand LoadCommand { get; }
        public RelayCommand PlayCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand StopCommand { get; }
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private double sliderPosition;
        private readonly ObservableCollection<string> inputPathHistory;
        private string lastPlayed;
        private float singleFrequency;
        private string singleNote;
        private string timerPosition;

        public MediaFoundationPlaybackViewModel()
        {
            inputPathHistory = new ObservableCollection<string>();
            LoadCommand = new RelayCommand(Load, () => IsStopped);
            PlayCommand = new RelayCommand(Play, () => !IsPlaying);
            PauseCommand = new RelayCommand(Pause, () => IsPlaying);
            StopCommand = new RelayCommand(Stop, () => !IsStopped);
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerOnTick;
            audioPlayback = new AudioPlayback();
            audioPlayback.BufferEventArgs += audioGraph_Buffer;
            TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
        }

        public bool IsPlaying => audioPlayback.PlaybackDevice != null && audioPlayback.PlaybackDevice.PlaybackState == PlaybackState.Playing;

        public bool IsStopped => audioPlayback.PlaybackDevice == null || audioPlayback.PlaybackDevice.PlaybackState == PlaybackState.Stopped;


        public IEnumerable<string> InputPathHistory => inputPathHistory;

        const double SliderMax = 10.0;

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (audioPlayback.FileStream != null)
            {
                sliderPosition = Math.Min(SliderMax, audioPlayback.FileStream.Position * SliderMax / audioPlayback.FileStream.Length);
                TimePosition = audioPlayback.FileStream.CurrentTime.ToString("mm\\:ss");
                OnPropertyChanged("SliderPosition");
            }
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
                    if (audioPlayback.FileStream != null)
                    {
                        var pos = (long)(audioPlayback.FileStream.Length * sliderPosition / SliderMax);
                        audioPlayback.FileStream.Position = pos; // media foundation will worry about block align for us
                    }
                    OnPropertyChanged("SliderPosition");
                }
            }
        }

        public float SingleFrequency
        {
            get => singleFrequency;
            set
            {
                if (singleFrequency != value)
                {
                    singleFrequency = value;
                    OnPropertyChanged("SingleFrequency");
                }
            }
        }
        public string SingleNote
        {
            get => singleNote;
            set
            {
                if (singleNote != value)
                {
                    singleNote = value;
                    OnPropertyChanged("SingleNote");
                }
            }
        }

        public int RequestFloatOutput
        {
            get => requestFloatOutput;
            set
            {
                if (requestFloatOutput != value)
                {
                    requestFloatOutput = value;
                    OnPropertyChanged("RequestFloatOutput");
                }
            }
        }

        private void SelectInputFile()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                if (TryOpenInputFile(ofd.FileName))
                {
                    TryOpenInputFile(ofd.FileName);
                }
            }
        }

        private bool TryOpenInputFile(string file)
        {
            bool isValid = false;
            try
            {
                using (var tempReader = new AudioFileReader(file))
                {
                    DefaultDecompressionFormat = tempReader.WaveFormat.ToString();
                    InputPath = file;
                    isValid = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Not a supported input file ({e.Message})");
            }
            return isValid;
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
            get => inputPath;
            set
            {
                if (inputPath != value)
                {
                    inputPath = value;
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

        private void Stop()
        {
            if (audioPlayback.PlaybackDevice != null)
            {
                audioPlayback.Stop();
            }
        }

        private void Pause()
        {
            if (audioPlayback.PlaybackDevice != null)
            {
                audioPlayback.Pause();
                OnPropertyChanged("IsPlaying");
                OnPropertyChanged("IsStopped");
            }
        }

        private void Play()
        {
            if (String.IsNullOrEmpty(InputPath))
            {
                MessageBox.Show("Select a valid input file or URL first");
                return;
            }
            if (audioPlayback.PlaybackDevice == null)
            {
                CreatePlayer();
            }
            if (lastPlayed != inputPath && audioPlayback.FileStream != null)
            {
                audioPlayback.CloseFile();
            }
            if (audioPlayback.FileStream == null)
            {
                audioPlayback.Load(InputPath);
                lastPlayed = inputPath;

            }
            audioPlayback.Play();
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("IsStopped");
            timer.Start();
        }

        private void CreatePlayer()
        {
            audioPlayback.CreateDevice();
            audioPlayback.PlaybackDevice.PlaybackStopped += WavePlayerOnPlaybackStopped;
        }

        void audioGraph_Buffer(object sender, BufferEventArgs e)
        {
            float freq = audioPlayback.pitch.Get(e.Buffer);
            if (freq != 0)
            {
                SingleFrequency = freq;
                SingleNote = audioPlayback.GetNote(freq);
            }
        }

        private void WavePlayerOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {

            if (audioPlayback.FileStream != null)
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
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("IsStopped");
        }

        private void Load()
        {
            if (audioPlayback.FileStream != null)
            {
                audioPlayback.CloseFile();
            }
            SelectInputFile();
        }

        public void Dispose()
        {
            audioPlayback.Dispose();
        }
    }

}