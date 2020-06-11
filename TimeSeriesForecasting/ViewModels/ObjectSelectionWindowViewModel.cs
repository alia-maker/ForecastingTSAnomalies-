using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.ModelBuilding;
using TimeSeriesForecasting.Views;

namespace TimeSeriesForecasting.ViewModels
{
    public class ObjectSelectionWindowViewModel : ViewModel
    {
        DBContext _dBContext;
       // private Window current;
        private string _currentObjectName;

        public string CurrentObjectName
        {
            get => _currentObjectName;
            set => Set(ref _currentObjectName, value);
        }
        private DateTime _begin = DateTime.Parse("01.01.2020");
        public DateTime Begin
        {
            get => _begin;
            set=> Set(ref _begin, value);
        }

        private DateTime _end = DateTime.Parse("01.03.2020");
        public DateTime End
        {
            get => _end;
            set => Set(ref _end, value);
        }

        public ObjectSelectionWindowViewModel() { }
        private XGBoostModel _xgbmodel;
        public ObjectSelectionWindowViewModel(DBContext dBContext, XGBoostModel xGBoost)
        {
            _xgbmodel = xGBoost;
            _dBContext = dBContext;
            ObjectNames = _dBContext.ObjectNames;
            CurrentObjectName = _dBContext.SelectedObject;
            _dBContext.OnRequestExecution += DisableButtons;
            _dBContext.OnRequestCompleted += EnableButtons;

            //GetObjectNames = new RelayCommand(async x => ObjectNames = await _dBContext.LoadObjectsAsync());
           // _dBContext.OnDataObjectLoaded += CloseWindow;
            LoadObjects = new RelayCommand(async x => ObjectNames = await _dBContext.LoadObjectsAsync());
            _dBContext.OnObjectNamesLoaded += SelectedObjectChanged;
            LoadObjectData = new RelayCommandParam<Window>(async win =>
            {
                win.Close();
                await _dBContext.LoadObjectDataAsync(CurrentObjectName, Begin, End);
               // _dBContext.SelectedObject = CurrentObjectName;
            });

            

            //DetectAnomaly = new RelayCommandParam<Window>(win =>
            //{
            //    win.DialogResult = true;
            //    win.Close();
            //});
            //CloseWindow = new RelayCommandParam<Window>(win =>
            //{
            //    win.DialogResult = false;
            //    win.Close();
            //});

        }
        
        //ICommand DetectAnomaly { get; }
        //ICommand CloseWindow { get; }
        //private void CloseWindow()
        //{
        //    current.Close();
        //}

        public ICommand OnLoad => new RelayCommandParam<ObjectSelectionWindow>(SetUpComplexControls);
        private Button _loadObjectsButton;
        private Button _loadDataObjectButton;
        private void SetUpComplexControls(ObjectSelectionWindow window)
        {
            _loadObjectsButton = window.LoadOblectsButton;
            _loadDataObjectButton = window.LoadDataObjectButton;        }

        private void DisableButtons()
        {
            _loadObjectsButton.IsEnabled = false;
            _loadDataObjectButton.IsEnabled = false;
        }
        private void EnableButtons()
        {
            _loadObjectsButton.IsEnabled = true;
            _loadDataObjectButton.IsEnabled = true;
        }

        private void SelectedObjectChanged()
        {
            CurrentObjectName = ObjectNames[0];
        }

        public ICommand LoadObjects { get; }
        public ICommand LoadObjectData { get; }
        // public ICommand GetObjectNames { get; }

        //public ICommand ExitCommand => new RelayCommandParam<Window>(win => win.Close());

        private TimeSeriesData _timeSeriesData;
        public TimeSeriesData TimeSeriesData
        {
            get => _timeSeriesData;
            set => Set(ref _timeSeriesData, value);
        }

        private List<string> _objectNames;

        public List<string> ObjectNames
        {
            get => _objectNames;
            set => Set(ref _objectNames, value);
        }
        

    }
}
