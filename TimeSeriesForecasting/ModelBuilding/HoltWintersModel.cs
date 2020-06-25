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
        public event Action ONConnectionStatusChanged;

        public async void Create(TimeSeriesData data)
        {
            await _createThread(data);
        }

        private async Task _createThread(TimeSeriesData data)
        {
            try
            {
                if (data.Points.Count > 0)
                {
                    if (_pythonManager.Process is null || _pythonManager.Process.HasExited)
                    _pythonManager.Process = Process.Start(startInfo);
                    _pythonManager.Send<TimeSeriesData>(data);

                    _dbContext.ConnectionStatus = "Данные успешно отправлены. Ожидание ответа...";
                }
                else
                {
                    _dbContext.ConnectionStatus = "При отправке возникла ошибка. Стек данных пуст.";
                }
            }
            catch (Exception e)
            {
               MessageBox.Show(e.ToString());
                _dbContext.ConnectionStatus = "При отправке возникла ошибка";
            }
            ONConnectionStatusChanged?.Invoke();
            try
            {
              

                var param = await Task.Run(() => _pythonManager.Receive<HoltWintersModelParameters>());
                if (param.model_created)
                {
                    var name = _dbContext.SelectedObject; //todo: ask user for model name
                    var fileData = new HoltWintersModelParams()
                    {
                        Name = name,
                        ModelParamData = param,
                        CreatedDate = DateTime.Now,
                        TypeModel = TypeModelEnum.HoltWinters,
                    };


                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "Models", TypeModelEnum.HoltWinters.ToString());
                    _fileWorker.CheckingFolder(folder);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Models", TypeModelEnum.HoltWinters.ToString(), _dbContext.SelectedObject + Guid.NewGuid() + ".json");


                    _fileWorker.Save(fileData, filePath);
                    MessageBox.Show("Модель сохранена");
                    _dbContext.ConnectionStatus = "Модель Хольта-Винтерса сохранена";
                    
                }
                else
                {
                    _dbContext.ConnectionStatus = "При создании модели Хольта-Винтерса возникли ошибки";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                _dbContext.ConnectionStatus = "При создании модели Хольта-Винтерса возникли ошибки";
            }
            ONConnectionStatusChanged();
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

            bool? res = false;
            try
            {

                

                var modelParams = _modelParamsReader.GetAllModelParams<HoltWintersModelParams>(Directory.GetCurrentDirectory(), TypeModelEnum.HoltWinters);

                var (vm, window) = _viewManager.GetWindow<ModelParamsHWSelectorVM, ModelParamsHWSelectorWindow>();
                vm.ModelParamsSource = modelParams;
                res = window.ShowDialog();

                if (res == true && _dbContext.TimeSeriesData.Points.Count > 0)
                {
                    if (_pythonManager.Process is null || _pythonManager.Process.HasExited)
                        _pythonManager.Process = Process.Start(startInfo);
                    

                    var HoltWintersParam = vm.ModelParamData.ModelParamData;
                    HoltWintersParam.Data = _dbContext.TimeSeriesData;
                    HoltWintersParam.SeriesType = SeriesType.ForecastingHoltWinters;
                    HoltWintersParam.ScalingFactor = _dbContext.ScalingFactorHoltWinters;
                    HoltWintersParam.PlotBrawser = _dbContext.PlotBrawser;
                    //var modelParams = _modelParamsReader.GetAllModelParams(Directory.GetCurrentDirectory(),  TypeModelEnum.HoltWinters);
                    //var (vm, window) = _viewManager.GetWindow<ModelParamsSelectorVM, ModelParamsSelectorWindow>();
                    //vm.ModelParamsSource = modelParams.ToList();
                    // var res = window.ShowDialog();

                    //var winterParam = vm.ModelParamData.ModelParamData as HoltWintersModelParameters;
                    //    string fullPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Models", "HoltWinters",
                    //"S01N00145U001N01D0035N01PAI____PI00a8a130cc-cc01-480f-970f-bf6fffb48e8f.json");
                    //var winterParam = _fileWorker.Read<HoltWintersModelParameters>(fullPath1,
                    //    "");
                    //var t = new AnalysisDataSend(winterParam, data, _dbContext.ScalingFactorHoltWinters);
                    _pythonManager.Send<HoltWintersModelParameters>(HoltWintersParam);
                    _dbContext.ConnectionStatus = "Данные для анализа успешно отправлены. Ожидание ответа...";
                    ONConnectionStatusChanged?.Invoke();
                }
                else
                {
                    _dbContext.ConnectionStatus = "Данные отправить не удалось";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                _dbContext.ConnectionStatus = "Данные отправить не удалось";
                ONConnectionStatusChanged();
            }
           // PlotData _tempdata;
            try
            {
                var analysis_data = await Task.Run(() => _pythonManager.Receive<MyClassDto>());
                var _tempData = PlotData.FromDto(analysis_data);
                _dbContext.ConnectionStatus = "Данные получены";
                ONConnectionStatusChanged?.Invoke();
                return await Task.FromResult(_tempData);
            }
            catch (Exception e)
            {
                _dbContext.ConnectionStatus = "Данные для анализа успешно отправлены. Ожидание ответа...";
                ONConnectionStatusChanged?.Invoke();
                MessageBox.Show(e.ToString());

            }


            return await Task.FromResult(new PlotData());

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
        public TimeSeriesData Data { get; set; }
        public SeriesType SeriesType { get; set; }
        public float ScalingFactor { get; set; }
        public bool PlotBrawser { get; set; }
        public override string ToString()
        {
            return $"Название объекта: {Name}; alpha: {alpha};betta: {betta};gamma: {gamma}; Длина сезона: {season_len}";
        }



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
