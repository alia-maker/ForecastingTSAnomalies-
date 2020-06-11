using System;
using System.Collections.Generic;
using System.IO;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using static TimeSeriesForecasting.ModelBuilding.HoltWintersModel;

namespace TimeSeriesForecasting.ModelBuilding
{
    public interface IModelParamsReader
    {
        List<T> GetAllModelParams<T>(string path, TypeModelEnum type);
    }

    public class ModelParamsReader : IModelParamsReader
    {
        private readonly IFileWorker _fileWorker;

        public ModelParamsReader(IFileWorker fileWorker)
        {
            _fileWorker = fileWorker;
        }

        public List<T> GetAllModelParams<T>(string path, TypeModelEnum type)
        {
            var result = new List<T>();
            var folder = Path.Combine(path, "Models", type.ToString());
            foreach (var filePath in Directory.EnumerateFiles(folder))
            {
                try
                {
                    var temp = _fileWorker.Read<T>(filePath, "");
                    result.Add(temp);
                }
                catch (Exception)
                {
                    // skip error file
                }
            }

            return result;
        }
    }


}
