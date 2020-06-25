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
    public class ModelParamsSelectorVM: ViewModel
    {
        private float _scalingFactor;
        public float ScalingFactor
        {
            get => _scalingFactor;
            set => Set(ref _scalingFactor, value);
        }
        private bool _plotBrawser;
        public bool PlotBrawser
        {
            get => _plotBrawser;
            set => Set(ref _plotBrawser, value);
        }

        private DBContext _dBContext; 
        public ModelParamsSelectorVM() { }
        public ModelParamsSelectorVM(DBContext dBContext)
        {
            ScalingFactor = (float)1.6;
            _dBContext = dBContext;
            DetectAnomaly = new RelayCommandParam<Window>(win =>
            {
                if (ModelParamData != null)
                {
                    _dBContext.ScalingFactorXGBoost = ScalingFactor;
                    _dBContext.PlotBrawser = PlotBrawser;
                    win.DialogResult = true;
                   
                    win.Close();
                }
                else
                {
                    MessageBox.Show("Модель не выбрана");
                }
            });
            CloseWindow = new RelayCommandParam<Window>(win =>
            {
                win.DialogResult = false;
                win.Close();
            });

        }

        public ICommand DetectAnomaly { get; }
        public ICommand CloseWindow { get; }
    

        private List<XGBoostModelParams> _modelParamsSource;
        public List<XGBoostModelParams> ModelParamsSource
        {
            get => _modelParamsSource;
            set => Set(ref _modelParamsSource, value);
        }

        private XGBoostModelParams _modelParamData;
        public XGBoostModelParams ModelParamData
        {
            get => _modelParamData;
            set => Set(ref _modelParamData, value);
        }
    }
}
