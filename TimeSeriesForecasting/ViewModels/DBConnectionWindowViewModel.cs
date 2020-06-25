using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;

namespace TimeSeriesForecasting.ViewModels
{
    public class DBConnectionWindowViewModel : ViewModel
    {

        private List<string> _tableNames;

        public List<string> TableNames
        {
            get => _tableNames;
            set => Set(ref _tableNames, value);
        }

        public Array AllAuthenticationTypes => Enum.GetValues(typeof(EnumAuthenticationType));
        private EnumAuthenticationType _selectedAuthenticationType;

        public EnumAuthenticationType SelectedAuthenticationType
        {
            get => _selectedAuthenticationType;
            set
            {
                Set(ref _selectedAuthenticationType, value);
                ChangeActivationFields();
            }
        }

        private void ChangeActivationFields()
        {
            switch (SelectedAuthenticationType)
            {
                case EnumAuthenticationType.Windows:
                    NameEnable = false;
                    _dbContext.IntegratedSecurity = true;
                    break;
                case EnumAuthenticationType.SQL_Server:
                    NameEnable = true;
                    _dbContext.IntegratedSecurity = false;

                    break;
                default:
                    NameEnable = false;
                    _dbContext.IntegratedSecurity = true;

                    break;
            }
        }


        private bool _nameEnable;
        public bool NameEnable
        {
            get =>_nameEnable;
            set => Set(ref _nameEnable, value);
        }
        private string _dataSource;
        private string _userID;
        private string _password;
        private string _initialCatalog;
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

        private IFileWorker _fileWorker;
        public DBConnectionWindowViewModel() { }
        public DBConnectionWindowViewModel(DBContext dBContext, IFileWorker fileWorker)
        {
            DataSource = "DESKTOP-SSLIKJH";
            UserID = "sa";
            Password = "1";
            InitialCatalog = "SEICDatabase";
            _dbContext = dBContext;
            _fileWorker = fileWorker;
            ReadFileData();
            
            SelectedAuthenticationType = EnumAuthenticationType.Windows;
            CheckConnection = new RelayCommand(x => _dbContext.CheckConnection(_connectionInfo = new DBConnectionInfo
            {
                DataSource = _dataSource.Trim(' '),
                UserID = _userID.Trim(' '),
                Password = _password.Trim(' '),
                InitialCatalog = _initialCatalog.Trim(' '),
            }
            ));

            CheckingSelectedFields = new RelayCommandParam<Window>(win => {
                var result = _dbContext.CheckSelectedFields(CurrentTableName,
                CurrentColumnNameTag, CurrentColumnNameTimestamp, CurrentColumnNameValue);
                if (result) win.Close();
                else MessageBox.Show("Соединение не успешно");
            }
            );


            CreateConnection = new RelayCommandParam<Window>(win => {

                var result = _dbContext.CreateConnection(_connectionInfo = new DBConnectionInfo
                {
                    DataSource = DataSource.Trim(' '),
                    UserID = UserID.Trim(' '),
                    Password = Password.Trim(' '),
                    InitialCatalog = InitialCatalog.Trim(' '),
                }
                );
                if (result)
                {
                    ConnectEnable = true;
                    TableNames = _dbContext.TableNames;
                }
                    //win.Close();
            }
            );

        }



        private List<string> _columnNames;

        public List<string> ColumnNames
        {
            get => _columnNames;
            set => Set(ref _columnNames, value);
        }


        private string _currentTableName;


        public string CurrentTableName
        {
            get => _currentTableName;
            set
            {
                Set(ref _currentTableName, value);
                if (_dbContext.LoadColumnTypes(CurrentTableName))
                    ColumnNames = _dbContext.ColumnNames;
            }
        }

        private string _currentColumnNameTag;

        public string CurrentColumnNameTag
        {
            get => _currentColumnNameTag;
            set
            {
                Set(ref _currentColumnNameTag, value);
            }
        }

        private string _currentColumnNameTimestamp;

        public string CurrentColumnNameTimestamp
        {
            get => _currentColumnNameTimestamp;
            set
            {
                Set(ref _currentColumnNameTimestamp, value);
            }
        }


        private string _currentColumnNameValue;

        public string CurrentColumnNameValue
        {
            get => _currentColumnNameValue;
            set
            {
                Set(ref _currentColumnNameValue, value);
            }
        }

        public ICommand CheckingSelectedFields { get; }




        private bool _connectEnable;
        public bool ConnectEnable
        {
            get => _connectEnable;
            set => Set(ref _connectEnable, value);
        }

        private void ReadFileData()
        {
            var info = _fileWorker.Read<DBConnectionInfo>("ConnectionInfo.json", "");
            if (info != null)
            {
                try
                {
                    UserID = info.UserID;
                    DataSource = info.DataSource;
                    Password = info.Password;
                    InitialCatalog = info.InitialCatalog;
                    CurrentColumnNameValue = info.ColumnValue;
                    CurrentTableName = info.Tablename;
                    CurrentColumnNameTag = info.ColumnTag;
                    CurrentColumnNameTimestamp = info.ColumnTimestamp;
                    TableNames = _dbContext.TableNames;
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

    }
    [TypeConverter(typeof(EnumDescriptionExtractor))]
    public enum EnumAuthenticationType
    {
        [Description("Проверка подлинности Windows")]
        Windows,
        [Description("Проверка подлинности SQL Server")]
        SQL_Server,
       
    }
}
