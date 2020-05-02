using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace TimeSeriesForecasting.HelpersLibrary
{
    public class JsonFileWorker : IFileWorker
    {
        public JsonFileWorker() { }

        public void Save<T>(T obj, string fileName)
        {
            string jsonString = JsonSerializer.Serialize<T>(obj);
            File.WriteAllText(fileName, jsonString);
        }

        public T Read<T>(string fileName)
        {
            string jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
