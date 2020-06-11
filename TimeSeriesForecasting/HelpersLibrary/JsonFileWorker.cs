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

        public void Save<T>(T obj, string filePath, string TypeModel = "")
        {
            var path_string = filePath;
            if (TypeModel != "")
            {
                string fullPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Models");
                DirectoryInfo dirInfo1 = new DirectoryInfo(fullPath1);
                if (!dirInfo1.Exists)
                {
                    dirInfo1.Create();
                }


                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Models", TypeModel);
                DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                path_string = Path.Combine(fullPath, filePath);
            }

            if(!Directory.Exists(Path.GetDirectoryName(path_string)))
                Directory.CreateDirectory(Path.GetDirectoryName(path_string));

            string jsonString = JsonSerializer.Serialize<T>(obj);
            File.WriteAllText(path_string, jsonString);
        }

        public T Read<T>(string fileName, string TypeModel)
        {
            string fullPath = fileName;
            if (TypeModel != "")
            {
                string[] paths = { "Models", TypeModel, fileName };
                fullPath = Path.Combine(paths);
            }

            if (!File.Exists(fullPath))
                return default(T);

            string jsonString = File.ReadAllText(fullPath);
            
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
