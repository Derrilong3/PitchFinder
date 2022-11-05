using NAudio.Wave;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PitchFinder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PitchFinder.ViewModels
{
    internal class MediaFoundationPlaybackViewModel : ViewModelBase, IDisposable
    {
        private int requestFloatOutput;
        private string inputPath;
        private string defaultDecompressionFormat;
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
        private PlotModel plotModel;
        double[] AudioValues;
        private bool timered;
        public IList<DataPoint> Points { get; private set; }
        public MediaFoundationPlaybackViewModel()
        {
            inputPathHistory = new ObservableCollection<string>();
            LoadCommand = new RelayCommand(Load, () => IsStopped);
            PlayCommand = new RelayCommand(Play, () => !IsPlaying);
            PauseCommand = new RelayCommand(Pause, () => IsPlaying);
            StopCommand = new RelayCommand(Stop, () => !IsStopped);
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += TimerOnTick;
            audioPlayback = new AudioPlayback();
            audioPlayback.BufferEventArgs += audioGraph_Buffer;
            audioPlayback.FftCalculated += AudioPlayback_FftCalculated;
            TimePosition = new TimeSpan(0, 0, 0).ToString("mm\\:ss");
            PlotModel = new PlotModel();
            plotModel.Series.Add(new LineSeries());
        }


        void audioGraph_Buffer(object sender, BufferEventArgs e)
        {
            //float freq = audioPlayback.pitch.Get(e.Buffer);
            //if (freq != 0)
            //{
            //    SingleFrequency = freq;
            //    SingleNote = audioPlayback.GetNote(freq);
            //}

            int bytesPerSamplePerChannel = audioPlayback.FileStream.WaveFormat.BitsPerSample / 8;
            int bytesPerSample = bytesPerSamplePerChannel * audioPlayback.FileStream.WaveFormat.Channels;
            int bufferSampleCount = e.Buffer.Length / bytesPerSample;

            if (bufferSampleCount >= AudioValues.Length)
            {
                bufferSampleCount = AudioValues.Length;
            }

            if (bytesPerSamplePerChannel == 2 && audioPlayback.FileStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt16(e.Buffer, i * bytesPerSample);
            }
            else if (bytesPerSamplePerChannel == 4 && audioPlayback.FileStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt32(e.Buffer, i * bytesPerSample);
            }
            else if (bytesPerSamplePerChannel == 4 && audioPlayback.FileStream.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToSingle(e.Buffer, i * bytesPerSample);
            }
            else
            {
                throw new NotSupportedException(audioPlayback.FileStream.WaveFormat.ToString());
            }

            if (timered)
            {
                var s = (LineSeries)PlotModel.Series[0];

                s.Points.Clear();

                double[] paddedAudio = FftSharp.Pad.ZeroPad(AudioValues);
                double[] fftMag = FftSharp.Transform.FFTmagnitude(paddedAudio);
                double[] freq = FftSharp.Transform.FFTfreq(audioPlayback.SampleRate, fftMag.Length);


                for (int i = 0; i < freq.Length; i++)
                {
                    s.Points.Add(new DataPoint(freq[i], fftMag[i]));
                }

                // find the frequency peak
                int peakIndex = 0;
                for (int i = 0; i < fftMag.Length; i++)
                {
                    if (fftMag[i] > fftMag[peakIndex])
                        peakIndex = i;
                }
                double fftPeriod = FftSharp.Transform.FFTfreqPeriod(audioPlayback.SampleRate, fftMag.Length);
                float peakFrequency = (float)(fftPeriod * peakIndex);

                SingleFrequency = peakFrequency;
                SingleNote = audioPlayback.GetNote(peakFrequency);

                PlotModel.InvalidatePlot(true);

                timered = false;
            }
        }


        private void AudioPlayback_FftCalculated(object? sender, FftEventArgs e)
        {

        }

        public bool IsPlaying => audioPlayback.PlaybackDevice != null && audioPlayback.PlaybackDevice.PlaybackState == PlaybackState.Playing;

        public bool IsStopped => audioPlayback.PlaybackDevice == null || audioPlayback.PlaybackDevice.PlaybackState == PlaybackState.Stopped;


        public IEnumerable<string> InputPathHistory => inputPathHistory;

        const double SliderMax = 10.0;

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            timered = true;
            if (audioPlayback.FileStream != null)
            {
                sliderPosition = Math.Min(SliderMax, audioPlayback.FileStream.Position * SliderMax / audioPlayback.FileStream.Length);
                TimePosition = audioPlayback.FileStream.CurrentTime.ToString("mm\\:ss");
                OnPropertyChanged("SliderPosition");
            }
        }

        public PlotModel PlotModel
        {
            get => plotModel;
            set
            {
                plotModel = value;
                OnPropertyChanged("PlotModel");
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

            if (string.IsNullOrEmpty(InputPath))
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
                AudioValues = new double[audioPlayback.SampleRate / 10];
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
                plotModel.Series.Clear();
                plotModel.Series.Add(new LineSeries());
                //plotModel.Axes.Clear();
                //plotModel.Axes.Add(new LinearAxis
                //{
                //    Position = AxisPosition.Left,
                //    zoo = 0.005d,
                //    ActualMinimum = 0,
                //});
                plotModel.InvalidatePlot(true);
            }
            SelectInputFile();
        }

        public void Dispose()
        {
            audioPlayback.Dispose();
        }
    }

}