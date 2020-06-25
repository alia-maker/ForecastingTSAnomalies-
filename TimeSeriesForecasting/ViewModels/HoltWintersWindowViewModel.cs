using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.ModelBuilding;

namespace TimeSeriesForecasting.ViewModels
{
    public class HoltWintersWindowViewModel : ViewModel
    { 
        public Array AllIntervalValues => Enum.GetValues(typeof(IntervalTypesEnum));// ExtractIntervalTypes();// Enum.GetValues(typeof(IntervalTypesEnum));

        //private static Array ExtractIntervalTypes()
        //{
        //    return Enum.GetValues(typeof(IntervalTypesEnum)).Cast<IntervalTypesEnum>()
        //        .Select(x => new
        //        {
        //            Value = x,
        //            Description = (Attribute.GetCustomAttribute(
        //                                    x.GetType().GetField(x.ToString()),
        //                                    typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description ?? x.ToString(),
        //        }).ToArray();
        //}

        private HoltWintersModel _model;
        private DBContext _dbContext;
        public HoltWintersWindowViewModel() { }

        public HoltWintersWindowViewModel(DBContext dbContext, HoltWintersModel model)
        {
            _model = model;
            _dbContext = dbContext;
            NumberOfValues = 3;
            ScalingFactor = (float)2;
            BuildHoltWintersModel = new RelayCommandParam<Window>(win =>
            {
                _dbContext.TimeSeriesData.SeriesType = SeriesType.BuilderHoltWinters;
                _dbContext.TimeSeriesData.NumberOfValues = NumberOfValues;
                _dbContext.ScalingFactorHoltWinters = ScalingFactor;
                _dbContext.TimeSeriesData.IntervalType = SelectedIntervalType;
                win.Close();
                _model.Create(_dbContext.TimeSeriesData);
                //win.Close();
                //open python script and send data there
            });
        }
      
        public ICommand BuildHoltWintersModel { get; }


        private int _numberOfOvalue;
        public int NumberOfValues
        {
            get => _numberOfOvalue;
            set => Set(ref _numberOfOvalue, value);
        }

        private float _scalingFactor;
        public float ScalingFactor
        {
            get => _scalingFactor;
            set => Set(ref _scalingFactor, value);
        }

        private IntervalTypesEnum _selectedIntervalType;

        public IntervalTypesEnum SelectedIntervalType
        {
            get => _selectedIntervalType;
            set => Set(ref _selectedIntervalType, value);
        }

    }

    [TypeConverter(typeof(EnumDescriptionExtractor))]
    public enum IntervalTypesEnum
    {
        [Description("в час")]
        Hour,
        [Description("в сутки")]
        Day,
        [Description("в неделю")]
        Week,
        [Description("в месяц")]
        Month,
    }
}
