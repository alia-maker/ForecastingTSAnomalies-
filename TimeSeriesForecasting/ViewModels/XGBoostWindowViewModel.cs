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
    public class XGBoostWindowViewModel: ViewModel
    {
        public Array AllIntervalValues => Enum.GetValues(typeof(IntervalTypesEnum));
        private XGBoostModel _model;
        private DBContext _dbContext;

        private int _numberOfOvalue;

        public int NumberOfValues
        {
            get => _numberOfOvalue;
            set => Set(ref _numberOfOvalue, value);
        }
        public XGBoostWindowViewModel() { }
        public ICommand BuildXGBoostModel { get; }
        public XGBoostWindowViewModel(DBContext dbContext, XGBoostModel model)
        {
            _model = model;
            _dbContext = dbContext;
            NumberOfValues = 3;
            BuildXGBoostModel = new RelayCommandParam<Window>(win =>
            {
                _dbContext.TimeSeriesData.SeriesType = SeriesType.BuilderXGBoost;
                _dbContext.TimeSeriesData.NumberOfValues = NumberOfValues;
                _dbContext.TimeSeriesData.IntervalType = SelectedIntervalType;
                win.Close();
                _model.Create(_dbContext.TimeSeriesData);
                //win.Close();
            });
        }

        private IntervalTypesEnum _selectedIntervalType;

        public IntervalTypesEnum SelectedIntervalType
        {
            get => _selectedIntervalType;
            set => Set(ref _selectedIntervalType, value);
        }
    }
}
