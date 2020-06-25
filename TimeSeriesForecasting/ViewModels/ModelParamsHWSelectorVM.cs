using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.ModelBuilding;

namespace TimeSeriesForecasting.ViewModels
{
    class ModelParamsHWSelectorVM: ViewModel
    {
        private float _scalingFactor;
        public float ScalingFactor
        {
            get => _scalingFactor;
            set => Set(ref _scalingFactor, value);
        }


        private bool _plotBrawser = true;
        public bool PlotBrawser
        {
            get => _plotBrawser;
            set => Set(ref _plotBrawser, value);
        }

        private DBContext _dBContent;
        public ModelParamsHWSelectorVM() { }
        public ModelParamsHWSelectorVM( DBContext dBContext)
        {
            _dBContent = dBContext;
            ScalingFactor = (float)2.2;
            DetectAnomaly = new RelayCommandParam<Window>(win =>
            {
                if (ModelParamData != null)
                {
                   
                    _dBContent.ScalingFactorHoltWinters = ScalingFactor;
                    _dBContent.PlotBrawser = PlotBrawser;
                    win.DialogResult = true;
                    win.Close();
                }
                else
                    MessageBox.Show("Модель не выбрана");
            });
            CloseWindow = new RelayCommandParam<Window>(win =>
            {
                win.DialogResult = false;
                win.Close();
            });

        }

        public ICommand DetectAnomaly { get; }
        public ICommand CloseWindow { get; }


        private List<HoltWintersModelParams> _modelParamsSource;
        public List<HoltWintersModelParams> ModelParamsSource
        {
            get => _modelParamsSource;
            set => Set(ref _modelParamsSource, value);
        }

        private HoltWintersModelParams _modelParamData;
        public HoltWintersModelParams ModelParamData
        {
            get => _modelParamData;
            set => Set(ref _modelParamData, value);
        }
    }
}
