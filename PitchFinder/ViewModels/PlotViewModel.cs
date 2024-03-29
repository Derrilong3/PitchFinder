﻿using OxyPlot;
using PitchFinder.Models;
using System.Collections.ObjectModel;

namespace PitchFinder.ViewModels
{
    class PlotViewModel : ToolViewModel
    {
        private GraphModel _model;

        public PlotViewModel() : base("Plot Window")
        {
            ContentId = "PlotTool";
            _model = new GraphModel();
        }

        public GraphModel Model { get { return _model; } }

        public PlotModel PlotModel
        {
            get => _model.PlotModel;
        }

        public ObservableCollection<NoteBox> ColorMulti
        {
            get => _model.ColorMulti;
        }

        protected override void OnVisibilityChanged()
        {
            if (IsVisible)
            {
                if (_model != null)
                {
                    _model.Init();
                    OnPropertyChanged(nameof(PlotModel));
                    OnPropertyChanged(nameof(ColorMulti));
                }
            }
            else
            {
                _model.Dispose();
            }
        }
    }
}
