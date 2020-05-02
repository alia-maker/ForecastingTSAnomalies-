using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.Views;

namespace TimeSeriesForecasting.ViewModels
{
    public class ObjectSelectionWindowViewModel : ViewModel
    {
        DBContext _dBContext;
        private Window current;
        private string _currentObjectName;

        public string CurrentObjectName
        {
            get => _currentObjectName;
            set => Set(ref _currentObjectName, value);
        }



        public ObjectSelectionWindowViewModel() { }

        public ObjectSelectionWindowViewModel(DBContext dBContext)
        {
            _dBContext = dBContext;
            ObjectNames = _dBContext.ObjectNames;
            //GetObjectNames = new RelayCommand(async x => ObjectNames = await _dBContext.LoadObjectsAsync());
           // _dBContext.OnDataObjectLoaded += CloseWindow;
            LoadObjects = new RelayCommand(async x => ObjectNames = await _dBContext.LoadObjectsAsync());

            LoadObjectData = new RelayCommandParam<Window>(async win =>
            {
                await _dBContext.LoadObjectDataAsync(CurrentObjectName);
                _dBContext.SelectedObject = CurrentObjectName;
                win.Close();
            });
        }

        private void CloseWindow()
        {
            current.Close();
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
