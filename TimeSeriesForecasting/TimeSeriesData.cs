using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using TimeSeriesForecasting.ViewModels;

namespace TimeSeriesForecasting
{
    
    public class TimeSeriesData
    {
        public string Name { get; set; }
        public List<Point> Points { get; set; } = new List<Point>();
        public SeriesType SeriesType { get; set; }
        public int NumberOfValues { get; set; }
        public IntervalTypesEnum IntervalType { get; set; }
    }

    public class Point
    {
        public DateTime Date { get; set; }
        public float Value { get; set; }
        public Point(DateTime date, float value)
        {
            Date = date;
            Value = value;
        }
    }

    public enum SeriesType
    {
        BuilderHoltWinters,
        BuilderXGBoost,
        ForecastingHoltWinters,
        ForecastingXGBoost,
    }
}
