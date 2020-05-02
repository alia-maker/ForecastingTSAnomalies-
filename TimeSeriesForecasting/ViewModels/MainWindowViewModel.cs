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

        public MainWindowViewModel(ViewManager viewManager, DBContext dbContext)
        {
            _viewManager = viewManager;
            _dbContext = dbContext;
            _dbContext.ONConnectionStatusChanged += ChangedStatus;
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


        private void DataObjectChanged()
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
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }
    }
}
