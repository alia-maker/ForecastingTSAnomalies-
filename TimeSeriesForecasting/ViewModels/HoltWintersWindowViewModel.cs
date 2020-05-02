using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private IModel _model;
        private DBContext _dbContext;
        public HoltWintersWindowViewModel() { }
        public HoltWintersWindowViewModel(DBContext dbContext, IModel model )
        {
            _model = model;
            _dbContext = dbContext;
            NumberOfValue = 5;
            BuildHoltWintersModel = new RelayCommandParam<Window>(win =>
            {
                _dbContext.TimeSeriesData.SeriesType = SeriesType.BuilderHoltWinters;
                _model.Create(_dbContext.TimeSeriesData);
                win.Close();
                //open python script and send data there
            });
        }
      
        public ICommand BuildHoltWintersModel { get; }


        private int _numberOfOvalue;
        public int NumberOfValue
        {
            get => _numberOfOvalue;
            set => Set(ref _numberOfOvalue, value);
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
