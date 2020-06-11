using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        
        public ModelParamsSelectorVM()
        {
            
            DetectAnomaly = new RelayCommandParam<Window>(win =>
            {
                win.DialogResult = true;
                win.Close();
            });
            CloseWindow = new RelayCommandParam<Window>(win =>
            {
                win.DialogResult = false;
                win.Close();
            });

        }

        ICommand DetectAnomaly { get; }
        ICommand CloseWindow { get; }
    

        private List<T> _modelParamsSource;
        public List<T> ModelParamsSource
        {
            get => _modelParamsSource;
            set => Set(ref _modelParamsSource, value);
        }

        private T _modelParamData;
        public T ModelParamData
        {
            get => _modelParamData;
            set => Set(ref _modelParamData, value);
        }
    }
}
