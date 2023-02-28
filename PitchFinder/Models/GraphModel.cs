using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Series;
using PitchFinder.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace PitchFinder.Models
{
    class GraphModel : ViewModelBase, IDisposable
    {
        private DispatcherOperation _task;
        private Chromagram _chromagram;
        private int _sampleRate;
        private readonly string[] _noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public ObservableCollection<NoteBox> ColorMulti { get; private set; }

        public GraphModel()
        {
            WeakReferenceMessenger.Default.Register<Messages.SampleRateChangedMessage>(this, SampleRateUpdated);
        }

        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
            }
        }

        private float _singleFrequency;
        public float SingleFrequency
        {
            get => _singleFrequency;
            set
            {
                if (_singleFrequency != value)
                {
                    _singleFrequency = value;
                    OnPropertyChanged("SingleFrequency");
                }
            }
        }

        private string _singleNote;
        public string SingleNote
        {
            get => _singleNote;
            set
            {
                if (_singleNote != value)
                {
                    _singleNote = value;
                    OnPropertyChanged("SingleNote");
                }
            }
        }

        public void Init()
        {
            _plotModel = new PlotModel();
            _plotModel.Series.Add(new LineSeries());
            _plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = 2000 });
            _plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Maximum = 0.05f });
            ColorMulti = new ObservableCollection<NoteBox>();
            _chromagram = new Chromagram();
            _chromagram.Initialize(_sampleRate);

            for (int i = 0; i < _noteNames.Length; i++)
            {
                ColorMulti.Add(new NoteBox() { Text = _noteNames[i], Color = Color.FromRgb(0, 0, 0) });
            }

            WeakReferenceMessenger.Default.Register<Messages.FFTChangedMessage>(this, FFTUpdated);
        }

        private void Update()
        {
            if (_plotModel == null)
                return;

            _chromagram.Initialize(_sampleRate);
            _plotModel.Series.Clear();
            _plotModel.Series.Add(new LineSeries());
            _plotModel.InvalidatePlot(true);
        }

        public void SampleRateUpdated(object obj, Messages.SampleRateChangedMessage message)
        {
            _sampleRate = message.Value;
            Update();
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
            double fftPeriod = FftSharp.Transform.FFTfreqPeriod(_sampleRate, message.Value.Y.Length);
            float peakFrequency = (float)Math.Round((fftPeriod * peakIndex) * 100f) / 100f;

            var chroma = _chromagram.GetChroma(message.Value.Y);
            int idx = 0;

            _task = App.Current.Dispatcher.BeginInvoke((System.Action)delegate
            {
                for (int i = 0; i < ColorMulti.Count; i++)
                {
                    if (chroma[i] > chroma[idx])
                        idx = i;

                    byte G = (byte)(255 * chroma[i]);
                    ColorMulti[i].Color = Color.FromRgb(0, G, 0);
                }

                SingleFrequency = peakFrequency;
                SingleNote = ColorMulti[idx].Text;
            });

            PlotModel.InvalidatePlot(true);
        }

        public void Dispose()
        {
            WeakReferenceMessenger.Default.Unregister<Messages.FFTChangedMessage>(this);
            _task?.Wait();
            ColorMulti = null;
            _plotModel = null;
            _chromagram = null;
        }
    }
}
