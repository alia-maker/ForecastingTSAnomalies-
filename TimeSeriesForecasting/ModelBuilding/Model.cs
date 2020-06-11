using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecasting.ModelBuilding
{
    public interface IModel
    {
        void Create(TimeSeriesData data);
        Task<PlotData> Forecast();
    }
}
