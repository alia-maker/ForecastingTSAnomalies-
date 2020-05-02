using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecasting.ModelBuilding
{
    public class HoltWintersModel : IModel
    {
        public void Create(TimeSeriesData data)
        {
            PythonManager _pythonManager = new PythonManager();
            _pythonManager.Send<TimeSeriesData>(data);
        }

        public void Forecast(TimeSeriesData data)
        {
            throw new NotImplementedException();
        }
    }
    public class HoltWintersModelParameters
    {
        public double Alpha { get; set; }
        public double Betta { get; set; }
        public double Gamma { get; set; }
    }
}
