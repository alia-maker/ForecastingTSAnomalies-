using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TimeSeriesForecasting.Views;
using TimeSeriesForecasting.ViewModels;


namespace TimeSeriesForecasting.ModelBuilding
{
    public class XGBoostModelParams
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public TypeModelEnum TypeModel { get; set; }
        public XGBoostModelParameters ModelParamData { get; set; }

        //public ModelParams(string name, TypeModelEnum type)
        //{
        //    Name = name;
        //    CreatedDate = DateTime.Now;
        //    TypeModel = type;
            
        //}
    }
    public class HoltWintersModelParams
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public TypeModelEnum TypeModel { get; set; }
        public HoltWintersModelParameters ModelParamData { get; set; }

        //public ModelParams(string name, TypeModelEnum type)
        //{
        //    Name = name;
        //    CreatedDate = DateTime.Now;
        //    TypeModel = type;

        //}
    }



    public partial class HoltWintersModel : IModel
    {
        IFileWorker _fileWorker;
        DBContext _dbContext;
        PythonManager _pythonManager;
        IModelParamsReader _modelParamsReader;
        private ProcessStartInfo startInfo = new ProcessStartInfo("script2.bat");
        //private Process _process;
        private ViewManager _viewManager;
        public HoltWintersModel(DBContext dbContext, IFileWorker fileWorker, PythonManager pythonManager, 
            IModelParamsReader modelParamsReader, ViewManager viewManager)
        {
            _viewManager = viewManager;
            _fileWorker = fileWorker;
            _dbContext = dbContext;
            _pythonManager = pythonManager;
            _modelParamsReader = modelParamsReader;
        }
        public async void Create(TimeSeriesData data)
        {
            //var thread = new Thread(() => _createThread(data));//создаем поток с анонимным делегатом
            //thread.Start();

            await _createThread(data);
            //ProcessStartInfo startInfo = new ProcessStartInfo("script2.bat");
            ////startInfo.WorkingDirectory = Path.GetDirectoryName(startInfo.FileName);
            //Process.Start(startInfo);
            //// PythonManager _pythonManager = new PythonManager();
            //_pythonManager.Send<TimeSeriesData>(data);
            //var param = _pythonManager.Receive<HoltWintersModelParameters>();
            ////JsonFileWorker fileWorker = new JsonFileWorker();
            //_fileWorker.Save(param, _dbContext.SelectedObject + ".json");
            //MessageBox.Show(param.alpha.ToString() + " " + param.betta + " " + param.gamma);
        }

        private async Task _createThread(TimeSeriesData data)
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo("script2.bat");
            //Process.Start(startInfo);
            //Thread.Sleep(5000);
            ////startInfo.WorkingDirectory = Path.GetDirectoryName(startInfo.FileName);
           
            //if (_pythonManager.Process is null || _pythonManager.Process.HasExited)
            //    _pythonManager.Process = Process.Start(startInfo);
            _pythonManager.Send<TimeSeriesData>(data);
            //var param = await _pythonManager.Receive<HoltWintersModelParameters>();
            var param = await Task.Run(() => _pythonManager.Receive<HoltWintersModelParameters>());
          if (param.model_created)
            {
                var name = _dbContext.SelectedObject; //todo: ask user for model name
                //var fileData = new ModelParams(name, TypeModelEnum.HoltWinters, param);
                //{
                //    ModelParamData = param,
                //};

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Models", TypeModelEnum.HoltWinters.ToString(), _dbContext.SelectedObject + Guid.NewGuid() + ".json");
                _fileWorker.Save(param, filePath);
                MessageBox.Show(param.alpha.ToString() + " " + param.betta + " " + param.gamma);
            }
            else
            {
                MessageBox.Show(param.model_created.ToString());
            }
        }

       


        //S01N00233U001N01D0035N01PAI____PI00

        public async Task<PlotData> Forecast()
        {
            //PlotData plotdata;
            _dbContext.TimeSeriesData.SeriesType = SeriesType.ForecastingHoltWinters;
            var plotdata = await _forecastThreadAsync(_dbContext.TimeSeriesData);
            //var thread = new Thread(() => plotdata = _forecastThread(data));//создаем поток с анонимным делегатом
            //thread.Start();
            return plotdata;
            //PythonManager _pythonManager = new PythonManager();

            //_pythonManager.Send<AnalysisDataSend>
            //    (new
            //    AnalysisDataSend(_fileWorker.Read<HoltWintersModelParameters>(_dbContext.SelectedObject + ".json"),
            //    data));
            //var analysis_data = _pythonManager.Receive<AnalysisDataReceive>();
            //if (str=="model_params")
            //_pythonManager.Send<HoltWintersModelParameters>(_fileWorker.Read<HoltWintersModelParameters>(_dbContext.SelectedObject + ".json"));
        }


        private async Task<PlotData> _forecastThreadAsync(TimeSeriesData data)
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo("script2.bat");
            ////startInfo.WorkingDirectory = Path.GetDirectoryName(startInfo.FileName);
            //Process.Start(startInfo);



            //if (_pythonManager.Process is null || _pythonManager.Process.HasExited)
            //    _pythonManager.Process = Process.Start(startInfo);



            //var modelParams = _modelParamsReader.GetAllModelParams(Directory.GetCurrentDirectory(),  TypeModelEnum.HoltWinters);
            //var (vm, window) = _viewManager.GetWindow<ModelParamsSelectorVM, ModelParamsSelectorWindow>();
            //vm.ModelParamsSource = modelParams.ToList();
            // var res = window.ShowDialog();

            //var winterParam = vm.ModelParamData.ModelParamData as HoltWintersModelParameters;
            string fullPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Models", "HoltWinters",
                "S01N00145U001N01D0035N01PAI____PI00a8a130cc-cc01-480f-970f-bf6fffb48e8f.json");
            var winterParam = _fileWorker.Read<HoltWintersModelParameters>(fullPath1,
                "");
            var t = new AnalysisDataSend(winterParam, data, _dbContext.ScalingFactorHoltWinters);
            _pythonManager.Send<AnalysisDataSend>(t);

            var analysis_data = await Task.Run(() => _pythonManager.Receive<MyClassDto>());
            var _tempData = PlotData.FromDto(analysis_data);

            return await Task.FromResult(_tempData);

        }
    }
    public class HoltWintersModelParameters
    {
        public string Name { get; set; }
        public bool model_created { get; set; }
        public double alpha { get; set; }
        public double betta { get; set; }
        public double gamma { get; set; }
        public int season_len { get; set; }
    }

    public class AnalysisDataSend
    {
        public object Model { get; set; }
        public TimeSeriesData Data { get; set; }
        public SeriesType SeriesType { get; set; }
        public float ScalingFactor { get; set; }
        public AnalysisDataSend(object model, TimeSeriesData data, float scalingFactor)
        {
            Model = model;
            Data = data;
            SeriesType = Data.SeriesType;
            ScalingFactor = scalingFactor;
            
        }
    }

    public class MyClassDto : Dictionary<string, DataPoint>
    {

    }

    public class PlotData
    {
        public List<DataPoint> Data { get; set; }

        public static PlotData FromDto(MyClassDto dto)
        {
            var temp = dto.Select(x =>
            {
                x.Value.Datetime = DateTime.Parse(x.Key);
                return x.Value;
            })
              .ToList();
            return new PlotData { Data = temp };
        }
        
    }

    public class DataPoint
    {
        public DateTime Datetime { get; set; }
        public float Real { get; set; }
        public float Model { get; set; }
        public float UpperBond { get; set; }
        public float LowerBond { get; set; }
    }



}
