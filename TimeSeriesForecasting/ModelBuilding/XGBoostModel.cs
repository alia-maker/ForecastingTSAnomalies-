using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.ViewModels;
using TimeSeriesForecasting.Views;


namespace TimeSeriesForecasting.ModelBuilding
{
    public class XGBoostModel: IModel
    {
        IFileWorker _fileWorker;
        DBContext _dbContext;
        PythonManager _pythonManager;
        ProcessStartInfo startInfo = new ProcessStartInfo("script2.bat");
        public XGBoostModel(DBContext dbContext, IFileWorker fileWorker, PythonManager pythonManager,
            ViewManager viewManager, IModelParamsReader modelParamsReader)
        {
            _modelParamsReader = modelParamsReader;
            _viewManager = viewManager;
            _fileWorker = fileWorker;
            _dbContext = dbContext;
            _pythonManager = pythonManager;
        }
        public async void Create(TimeSeriesData data)
        {
            data.Name = _dbContext.SelectedObject;
            await _createThreadAsync(data);
        }
        public event Action ONConnectionStatusChanged;
        private async Task _createThreadAsync(TimeSeriesData data)
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
                    _dbContext.ConnectionStatus = "При отправке возникла ошибка";
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                _dbContext.ConnectionStatus = "При отправке возникла ошибка";
            }
           // _dbContext.ConnectionStatus = "Данные успешно отправлены. Ожидание ответа...";
            ONConnectionStatusChanged();
            try
            { 
                var param = await Task.Run(() => _pythonManager.Receive<XGBoostModelParameters>());

                if (param.ModelPath != null)
                {
                    var name = _dbContext.SelectedObject;
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "Models", TypeModelEnum.XGBoost.ToString());
                    _fileWorker.CheckingFolder(folder);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Models", TypeModelEnum.XGBoost.ToString(), _dbContext.SelectedObject + Guid.NewGuid() + ".json");
                    
                    var fileData = new XGBoostModelParams()
                    {
                        Name = name,
                        ModelParamData = param,
                        TypeModel = TypeModelEnum.XGBoost,
                        CreatedDate = DateTime.Now,
                        
                    };

                    _fileWorker.Save(fileData, filePath);
                    MessageBox.Show("Модель сохранена");
                    _dbContext.ConnectionStatus = "Модель XGBoost сохранена";
                }
                else
                {
                    _dbContext.ConnectionStatus = "Модель XGBoost не сохранена";
                }

            }
            catch(Exception e)
            {
                //MessageBox.Show(e.ToString());
                _dbContext.ConnectionStatus = "При создании модели XGBoost возникли ошибки";
                MessageBox.Show(e.ToString());

            }
            ONConnectionStatusChanged();
        }


        //S01N00233U001N01D0035N01PAI____PI00

        public async Task<PlotData> Forecast()
        {
            //PlotData plotdata;
            var plotdata = await _forecastThreadAsync();
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

        private ViewManager _viewManager;
        private IModelParamsReader _modelParamsReader;


        private async Task<PlotData> _forecastThreadAsync()
        {
            bool? res = false;
            try
            {
                //ProcessStartInfo startInfo = new ProcessStartInfo("script2.bat");
                ////startInfo.WorkingDirectory = Path.GetDirectoryName(startInfo.FileName);
                //Process.Start(startInfo);

               


                //modelparams = list
                var modelParams = _modelParamsReader.GetAllModelParams<XGBoostModelParams>(Directory.GetCurrentDirectory(), TypeModelEnum.XGBoost);

                var (vm, window) = _viewManager.GetWindow<ModelParamsSelectorVM, ModelParamsSelectorWindow>();
                vm.ModelParamsSource = modelParams;
                res = window.ShowDialog();

                if (res == true && _dbContext.TimeSeriesData.Points.Count>0)
                {
                    if (_pythonManager.Process is null || _pythonManager.Process.HasExited)
                        _pythonManager.Process = Process.Start(startInfo);
                    var xGBoostParam = vm.ModelParamData.ModelParamData;

                    // if (_pythonManager.Process is null || _pythonManager.Process.HasExited)
                    //     _pythonManager.Process = Process.Start(startInfo);


                    xGBoostParam.Data = _dbContext.TimeSeriesData;
                    xGBoostParam.SeriesType = SeriesType.ForecastingXGBoost;
                    xGBoostParam.ScalingFactor = _dbContext.ScalingFactorXGBoost;
                    xGBoostParam.PlotBrawser = _dbContext.PlotBrawser;
                    //string fullPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Models", "XGBoost",
                    //   "S01N00145U001N01D0035N01PAI____PI002d106784-10dd-4eab-ad9b-4342b3b7fc89.json");
                    //var param = _fileWorker.Read<XGBoostModelParameters>(fullPath1,
                    //    "");
                    //param.Data = _dbContext.TimeSeriesData;
                    //param.SeriesType = SeriesType.ForecastingXGBoost;

                    _pythonManager.Send<XGBoostModelParameters>(xGBoostParam);
                    _dbContext.ConnectionStatus = "Данные для анализа успешно отправлены. Ожидание ответа...";
                }
                else
                {
                    _dbContext.ConnectionStatus = "Данные отправить не удалось";
                }
                //_dbContext.ConnectionStatus = "Данные для анализа успешно отправлены. Ожидание ответа...";
                ONConnectionStatusChanged();
            }
            catch(Exception e)
            {
                //MessageBox.Show(e.ToString());
                _dbContext.ConnectionStatus = "Данные отправить не удалось";
                MessageBox.Show(e.ToString());

                ONConnectionStatusChanged();
            }
            var _tempData = new PlotData();
            
         
            if (res==true)
            try
            {
               // var analysis_data = await _pythonManager.Receive<MyClassDto>();
                var analysis_data = await Task.Run(() => _pythonManager.Receive<MyClassDto>());
                _tempData = PlotData.FromDto(analysis_data);
                _dbContext.ConnectionStatus = "Данные приняты";
                ONConnectionStatusChanged();
                return await Task.FromResult(_tempData);
            }
            catch (Exception e)
            {
               // MessageBox.Show("При чтении данных произошла ошибка");
                    MessageBox.Show(e.ToString());

                }

            return await Task.FromResult(_tempData);
        }
    }

    public class XGBoostModelParameters
    {
        public string Name { get; set; }
        public float Deviation { get; set; }
        public string ModelPath { get; set; }
        public TimeSeriesData Data { get; set; }
        public SeriesType SeriesType { get; set; }
        public float ScalingFactor { get; set; }
        public bool PlotBrawser { get; set; }

        public override string ToString()
        {
            return $"Название объекта: {Name}; Отклонение: {Deviation}; Путь к файлу: {ModelPath}";
        }
        //public XGBoostModelParameters(float deviation, string model, TimeSeriesData data, SeriesType seriesType, string name)
        //{
        //    Deviation = deviation;
        //    ModelPath = model;
        //    Name = name;
        //    Data = data;
        //    SeriesType = seriesType;
        //}

    }




}
