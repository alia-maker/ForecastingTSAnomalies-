using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;

namespace TimeSeriesForecasting.ViewModels
{
    public class DBConnectionWindowViewModel : ViewModel
    {
        private string _dataSource = "DESKTOP-SSLIKJH";
        private string _userID = "sa";
        private string _password = "1";
        private string _initialCatalog = "SEICDatabase";
        public string DataSource
        {
            get => _dataSource;
            set => Set(ref _dataSource, value);
        }

        public string UserID
        {
            get => _userID;
            set => Set(ref _userID, value);
        }
        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }
        public string InitialCatalog
        {
            get => _initialCatalog;
            set => Set(ref _initialCatalog, value);
        }

        private DBConnectionInfo _connectionInfo;

        public ICommand CheckConnection { get; }
        public ICommand CreateConnection { get; }
        private readonly DBContext _dbContext;


        public DBConnectionWindowViewModel() { }
        public DBConnectionWindowViewModel(DBContext dBContext)
        {
            _dbContext = dBContext;
            CheckConnection = new RelayCommand(x => _dbContext.CheckConnection(_connectionInfo = new DBConnectionInfo
            {
                DataSource = _dataSource,
                UserID = _userID,
                Password = _password,
                InitialCatalog = _initialCatalog,
            }
            ));
            CreateConnection = new RelayCommand(x => _dbContext.CreateConnection(_connectionInfo = new DBConnectionInfo
            {
                DataSource = DataSource,
                UserID = UserID,
                Password = Password,
                InitialCatalog = InitialCatalog,
            }
            ));

        }
    }
}
