using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Series;
using PitchFinder.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace PitchFinder.Models
{
    class GraphModel : ViewModelBase
    {
        private PlotModel plotModel;
        private float singleFrequency;
        private string singleNote;
        private Chromagram chromagram;
        private int sampleRate;
        private string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public ObservableCollection<NoteBox> ColorMulti { get; private set; }

        public GraphModel()
        {
            plotModel = new PlotModel();
            plotModel.Series.Add(new LineSeries());
            plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = 2000 });
            plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Maximum = 0.05f });
            ColorMulti = new ObservableCollection<NoteBox>();
            chromagram = new Chromagram();

            for (int i = 0; i < noteNames.Length; i++)
            {
                ColorMulti.Add(new NoteBox() { Text = noteNames[i], Color = Color.FromRgb(0, 0, 0) });
            }

            WeakReferenceMessenger.Default.Register<Messages.FFTChangedMessage>(this, FFTUpdated);
            WeakReferenceMessenger.Default.Register<Messages.SampleRateChangedMessage>(this, SampleRateUpdated);
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

        private void Init()
        {
            chromagram.Initialize(sampleRate);
            plotModel.Series.Clear();
            plotModel.Series.Add(new LineSeries());
            plotModel.InvalidatePlot(true);
        }

        public void SampleRateUpdated(object obj, Messages.SampleRateChangedMessage message)
        {
            sampleRate = message.Value;
            Init();
        }

        public void FFTUpdated(object obj, Messages.FFTChangedMessage message)
        {
            var s = (LineSeries)PlotModel.Series[0];

            s.Points.Clear();

            try
            {
                for (int i = 0; i < message.Value.X.Length; i++)
                {
                    s.Points.Add(new DataPoint(message.Value.X[i], message.Value.Y[i]));
                }
            }
            catch
            {
                return;
            }

            //find the frequency peak
            int peakIndex = 0;
            for (int i = 0; i < message.Value.Y.Length; i++)
            {
                if (message.Value.Y[i] > message.Value.Y[peakIndex])
                    peakIndex = i;
            }
            double fftPeriod = FftSharp.Transform.FFTfreqPeriod(sampleRate, message.Value.Y.Length);
            float peakFrequency = (float)Math.Round((fftPeriod * peakIndex) * 100f) / 100f;

            var chroma = chromagram.GetChroma(message.Value.Y);

            App.Current.Dispatcher.BeginInvoke((System.Action)delegate
            {
                for (int i = 0; i < ColorMulti.Count; i++)
                {
                    byte G = (byte)(255 * chroma[i]);
                    ColorMulti[i].Color = Color.FromRgb(0, G, 0);
                }

                SingleFrequency = peakFrequency;
                SingleNote = ColorMulti.MaxBy(x => x.Color.G).Text;
            });

            PlotModel.InvalidatePlot(true);
        }
    }
}
