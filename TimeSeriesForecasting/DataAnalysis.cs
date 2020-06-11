using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecasting
{
    public class DataAnalysis
    {
        public TimeSeriesData Predicted;
        public TimeSeriesData RealData;
        public TimeSeriesData Anomalies;
        public TimeSeriesData UpperBounds;
        public TimeSeriesData LowerBounds;
    }
}
