﻿using OxyPlot;
using PitchFinder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchFinder.ViewModels
{
    class PlotViewModel : ToolViewModel
    {
        private GraphModel _model;

        public PlotViewModel() : base("Plot Window")
        {
            ContentId = "PlotTool";
            _model= new GraphModel();
        }

        public PlotModel PlotModel
        {
            get => _model.PlotModel;
        }

        public ObservableCollection<NoteBox> ColorMulti
        {
            get => _model.ColorMulti;
        }

        public float SingleFrequency
        {
            get => _model.SingleFrequency;
        }

        public string SingleNote
        {
            get => _model.SingleNote;
        }
    }
}
