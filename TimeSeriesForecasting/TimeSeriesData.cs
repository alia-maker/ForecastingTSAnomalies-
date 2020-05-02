using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecasting
{
    public class TimeSeriesData
    {
        public List<Point> Points = new List<Point>();
        public SeriesType SeriesType;
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
