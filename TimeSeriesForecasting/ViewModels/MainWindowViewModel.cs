using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.Views;
using ScottPlot;
using TimeSeriesForecasting.ModelBuilding;
using System.Windows.Controls;
using System.Drawing;

namespace TimeSeriesForecasting.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        //todo Что хранит в себе главное окно: Datagrid c (x,y); точки для отрисовк; подключение к бд  
        private DataTable _table;
        public DataTable Table
        {
            get => _table;
            set => Set(ref _table, value);
        }

        private string _selectedObject;
        public string SelectedObject
        {
            get => _selectedObject;
            set => Set(ref _selectedObject, value);
        }
        public MainWindowViewModel() { }
        private HoltWintersModel _hwmodel;
        private XGBoostModel _xgbmodel;
        public MainWindowViewModel(ViewManager viewManager, DBContext dbContext, HoltWintersModel hmodel, XGBoostModel xmodel)
        {
            _viewManager = viewManager;
            _dbContext = dbContext;
            _hwmodel = hmodel;
            _xgbmodel = xmodel;
            _dbContext.ONConnectionStatusChanged += ChangedStatus;
            _xgbmodel = xmodel;
            _xgbmodel.ONConnectionStatusChanged += ChangedStatus;
            _dbContext.OnSelectedObjectChanged += SelectedObjectChanged;
            _dbContext.OnDataObjectLoaded += DataObjectChanged;
            StatusMessage = _dbContext.ConnectionStatus;
            SelectedObject = _dbContext.SelectedObject;
            
            //CreateConnectWithDatabase = new RelayCommand(x => DBContext = new DBContext());
        }


        private List<Point> _timeSeriesData;

        public List<Point> TimeSeriesData
        {
            get => _timeSeriesData;
            set => Set(ref _timeSeriesData, value);
        }


        private StylePlotterEnum _plotstyle;

        public ICommand SetLightColor => new RelayCommand(() =>
        {
            _plotstyle = StylePlotterEnum.Light;
            PlotterStyleChange();
        });

        public ICommand SetGrayColor => new RelayCommand(() =>
        {
            _plotstyle = StylePlotterEnum.Gray;
            PlotterStyleChange();
        });

        public ICommand SetBlackColor => new RelayCommand(() =>
        {
            _plotstyle = StylePlotterEnum.Black;
            PlotterStyleChange();
        });

        public ICommand SetBlueColor => new RelayCommand(() =>
        {
            _plotstyle = StylePlotterEnum.Blue;
            PlotterStyleChange();
        });

        private void PlotterStyleChange()
        {
            switch (_plotstyle)
            {
                case StylePlotterEnum.Light:
                    _plotter.plt.Style(ScottPlot.Style.Light2);
                    
                    break;
                case StylePlotterEnum.Gray:
                    _plotter.plt.Style(ScottPlot.Style.Gray1);
                    break;
                case StylePlotterEnum.Blue:
                    _plotter.plt.Style(ScottPlot.Style.Blue1);
                    break;
                case StylePlotterEnum.Black:
                    _plotter.plt.Style(ScottPlot.Style.Black);
                    break;
                default:
                    _plotter.plt.Style(ScottPlot.Style.Seaborn);
                    break;
            }
            _plotter.Render();
        }
        private void DataObjectChanged()
        {
            if (_dbContext.TimeSeriesData.Points.Count > 0)
            {
                TimeSeriesData = _dbContext.TimeSeriesData.Points;
                var temp = TimeSeriesData
                    .Select(x => new { date = Convert.ToDateTime(x.Date), y = Convert.ToDouble(x.Value) })
                    .OrderBy(row => row.date)
                    .ToList();

                var xDate = temp.Select(x => x.date.ToOADate()).ToArray();
                var Values = temp.Select(row => row.y).ToArray();

                _plotter.plt.Clear();
                _plotter.plt.PlotScatter(xDate, Values);

                _plotter.plt.Ticks(dateTimeX: true);

                _plotter.plt.YLabel("Pressure");

                //switch (plotSeries.SeriesType)
                //{
                //    case SeriesType.Marker:
                //        //wpfPlot1.dsadas
                //        break;
                //}

                _plotter.Render();
            }
            else
            {
                _plotter.plt.Clear();
                _plotter.Render();
            }

        }

        private void ChangedStatus()
        {
            StatusMessage = _dbContext.ConnectionStatus;
        }

        private void SelectedObjectChanged()
        {
            SelectedObject = _dbContext.SelectedObject;
        }
        private readonly ViewManager _viewManager;
        private readonly DBContext _dbContext;

        public ICommand DBConnectionWindowOpen => new RelayCommand(() =>
        {
            var (vm, window) = _viewManager.GetWindow<DBConnectionWindowViewModel, DBConnectionWindow>();
            var res = window.ShowDialog();
        });

        public ICommand OpenHoltWintersWindow => new RelayCommand(() =>
        {
            var (vm, window) = _viewManager.GetWindow<HoltWintersWindowViewModel, HoltWintersWindow>();
            var res = window.ShowDialog();
        });

        private WpfPlot _plotter;
        private DataGrid _dataGrid;

        public ICommand ObjectSelectionWindowOpen => new RelayCommand(() =>
        {
            var (vm, window) = _viewManager.GetWindow<ObjectSelectionWindowViewModel, ObjectSelectionWindow>();
            var res = window.ShowDialog();
        });

        public ICommand CreateConnectWithDatabase { get; }

        public ICommand OnLoad => new RelayCommandParam<MainWindow>(SetUpComplexControls);

        private void SetUpComplexControls(MainWindow window)
        {
            _plotter = window.wpfPlot1;
            _dataGrid = window.DataGrid;
            CreateFirstMessage();
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        private PlotData _plotData;
        public ICommand SearchAnomalyHoltWintersModel => new RelayCommand(async () =>
        {
            //_dbContext.TimeSeriesData.SeriesType = SeriesType.ForecastingHoltWinters;
            _plotData = await _hwmodel.Forecast( );
            PlotDataChanged();
        });
        public ICommand OpenXGBoostWindow => new RelayCommand(() =>
        {
            var (vm, window) = _viewManager.GetWindow<XGBoostWindowViewModel, XGBoostWindow>();
            var res = window.ShowDialog();
        });


        public ICommand SearchAnomalyXGBoostModel => new RelayCommand(async () =>
        {
            _plotData = await _xgbmodel.Forecast();
            PlotDataChanged();
        });
        private void PlotDataChanged()
        {
            if (_plotData!=null)
            {
                _plotter.plt.Clear();
                _plotter.plt.YLabel("Pressure");


                var xDate = _plotData.Data.Select(x => x.Datetime.ToOADate()).ToArray();
                var Values = _plotData.Data.Select(x => (Double)x.Real).ToArray();
                _plotter.plt.PlotScatter(xDate, Values, label: "Реальные данные");

                var mValues = _plotData.Data.Select(x => (Double)x.Model).ToArray();
                _plotter.plt.PlotScatter(xDate, mValues, label: "Модель");

                var upValues = _plotData.Data.Select(x => (Double)x.UpperBond).ToArray();
                _plotter.plt.PlotScatter(xDate, upValues, color: Color.Orange, lineWidth: 1, markerSize: 0, 
                    lineStyle: LineStyle.DashDot,
                    label: "Верхняя граница");

                var lValues = _plotData.Data.Select(x => (Double)x.LowerBond).ToArray();
                _plotter.plt.PlotScatter(xDate, lValues, color: Color.Orange, lineWidth: 1, markerSize: 0, 
                    lineStyle: LineStyle.DashDot, label: "Нижняя граница");

                _plotter.plt.PlotFill(xDate, lValues, xDate, upValues, fillAlpha: .5);

                var anomaly = _createAnimalyPoints();
                var aDate = anomaly.Select(x => x.Date.ToOADate()).ToArray();
                var aValues = anomaly.Select(x => (Double)x.Value).ToArray();
                _plotter.plt.PlotScatter(aDate, aValues, color: Color.Red, lineWidth: 0);

                _plotter.plt.Ticks(dateTimeX: true);

                _plotter.Render();
            }
            else
            {
                _plotter.plt.Clear();
                _plotter.Render();
            }
        }
        private List<Point> _createAnimalyPoints()
        {
            List<Point> anomaly = new List<Point>();
            for (int i = 0; i < _plotData.Data.Count; i++)
            {
                if (_plotData.Data[i].Real > _plotData.Data[i].UpperBond ||
                    _plotData.Data[i].Real < _plotData.Data[i].LowerBond)
                    anomaly.Add(new Point(_plotData.Data[i].Datetime,_plotData.Data[i].Real));
            }
            return anomaly;
        }

        private void CreateFirstMessage()
        {
            // Code from /src/ScottPlot.Demo/Experimental/CustomPlottables.cs
            _plotter.plt.Style(ScottPlot.Style.Light2);

            // rather than call Plot.Text(), create the Plottable object manually
            var customPlottable = new PlottableText(text: "Добро пожаловать! ", x: 0, y: 0,
                color: System.Drawing.Color.Gray, fontName: "arial", fontSize: 16,
                bold: true, label: "", alignment: TextAlignment.middleCenter,
                rotation: 0, frame: false, frameColor: System.Drawing.Color.Green);
           
            // you can access properties which may not be exposed by a Plot method
            customPlottable.rotation = 0;

            // add the custom plottable to the list of plottables like this
            List<Plottable> plottables = _plotter.plt.GetPlottables();
            plottables.Add(customPlottable);
            _plotter.Render();
        }

    }
    public enum StylePlotterEnum
    {
        Light,
        Gray,
        Blue,
        Black,
    }
}
