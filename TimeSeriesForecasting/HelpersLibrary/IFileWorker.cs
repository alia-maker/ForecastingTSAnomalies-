using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecasting.HelpersLibrary
{
    public interface IFileWorker
    {
        void Save<T>(T obj, string fileName, string TypeModel="");
        T Read<T>(string fileName, string TypeModel);
    }
}
