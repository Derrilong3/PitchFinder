using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Series;
using PitchFinder.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace PitchFinder.Models
{
    class GraphModel : ViewModelBase, IDisposable
    {
        private DispatcherOperation _task;
        private Chromagram _chromagram;
        private LineSeries _lineSeries;
        private int _sampleRate;
        private readonly string[] _noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private double[] Xs;

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
            _lineSeries = new LineSeries();
            _plotModel.Series.Add(_lineSeries);
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
            _lineSeries = new LineSeries();
            _plotModel.Series.Add(_lineSeries);
            _plotModel.InvalidatePlot(true);
        }

        public void SampleRateUpdated(object obj, Messages.SampleRateChangedMessage message)
        {
            _sampleRate = message.Value;

            double[] freqs = new double[2048];

            double fftPeriodHz = (double)_sampleRate / (2048) / 2;

            for (int i = 0; i < 2048; i++)
                freqs[i] = i * fftPeriodHz;

            Xs = freqs;

            Update();
        }

        public async void FFTUpdated(object obj, Messages.FFTChangedMessage message)
        {
            //find the frequency peak
            int peakIndex = 0;
            for (int i = 0; i < message.Value.Fft.Length; i++)
            {
                if (message.Value.Fft[i] > message.Value.Fft[peakIndex])
                    peakIndex = i;
            }
            double fftPeriod = FftSharp.Transform.FFTfreqPeriod(_sampleRate, message.Value.Fft.Length);
            float peakFrequency = (float)Math.Round((fftPeriod * peakIndex) * 100f) / 100f;

            var chroma = _chromagram.GetChroma(message.Value.Fft);
            int idx = 0;

            var temp = new List<DataPoint>();
            for (int i = 0; i < Xs.Length; i++)
            {
                temp.Add(new DataPoint(Xs[i], message.Value.Fft[i]));
            }

            await App.Current.Dispatcher.BeginInvoke((System.Action)delegate
            {
                _lineSeries.Points.Clear();
                _lineSeries.Points.AddRange(temp);

                PlotModel.InvalidatePlot(true);
            });

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
