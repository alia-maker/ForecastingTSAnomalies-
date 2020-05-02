using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TimeSeriesForecasting.HelpersLibrary;
//using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace TimeSeriesForecasting.DataBase
{
    public class DBContext : IDisposable
    {
        private SqlConnection _connection;

        public string SelectedObject { get; set; }

        public event Action ONConnectionStatusChanged;

        public event Action OnSelectedObjectChanged;

        public event Action OnDataObjectLoaded;




        private static readonly string _fileName = "ConnectionInfo.json";
        IFileWorker _fileWorker;

        public void CheckConnection(DBConnectionInfo _connectionInfo)
        {
            ConnectionStatus = "";
            _openDBConnection(_connectionInfo);
            _connection.Close();
            MessageBox.Show(ConnectionStatus);
        }


        public void CreateConnection(DBConnectionInfo _connectionInfo)
        {
            _fileWorker.Save(_connectionInfo, _fileName);
            ConnectionStatus = "";
            _tryConnect(_connectionInfo);
            MessageBox.Show(ConnectionStatus);
            ONConnectionStatusChanged();
        }


        //todo : создаем либо прежнее соединение, либо новое (которое сохраняем в файле) 
        public DBContext(IFileWorker fileWorker)
        {
            _fileWorker = fileWorker;
            //попытка восстановить последнее соединение
            _tryLastConnect();
        }

        private void _tryLastConnect()
        {
            var connectPath = _fileName;
            if (File.Exists(connectPath))
            {
                ConnectionStatus = "";
                _connectionInfo = _fileWorker.Read<DBConnectionInfo>(_fileName);
                _openDBConnection(_connectionInfo);

            }
            else
                _connectionStatus = "Установите параметры соединения с базой данных";

            //ONConnectionStatusChanged();
        }


        private void _tryConnect(DBConnectionInfo _connectionInfo)
        {
            _openDBConnection(_connectionInfo);
            //_connectionStatus = _checkConnectionStatus;
        }





        // private string _checkConnectionStatus;
        //public string CheckConnectionStatus
        //{
        //    get => _checkConnectionStatus;
        //    set => _checkConnectionStatus = value;
        //} 
        private string _connectionStatus;
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => _connectionStatus = value;
        }

        private void _openDBConnection(DBConnectionInfo _connectionInfo)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = _connectionInfo.DataSource,
                UserID = _connectionInfo.UserID,
                IntegratedSecurity = false,
                Password = _connectionInfo.Password,
                InitialCatalog = _connectionInfo.InitialCatalog,

            };

            _connection = new SqlConnection(builder.ConnectionString);
            try
            {
                _connection.Open();
                ConnectionStatus = "Соединение успешно";
            }
            catch (Exception e)
            {
                ConnectionStatus = "Соединение не установлено";
                MessageBox.Show(e.ToString());
            }

        }

        private DBConnectionInfo _connectionInfo;


        public void Dispose()
        {
            _fileWorker.Save(_objectNames, "fdsfsd");
            _connection?.Dispose();
        }

        private List<string> _objectNames = new List<string>();

        public List<string> ObjectNames
        {
            get => _objectNames;
        }

        public async Task<List<string>> LoadObjectsAsync()
        {
            try
            {
                //if (_objectNames.Count==0)
                // {
                //_objectNames = new List<string>();

                var sql = " SELECT DISTINCT Ev_Info FROM SEICDatabase.dbo.SEICEVENTHISTORY" +
                          " WHERE right(Ev_Info,5) = '_PI00' ";
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.CommandTimeout = 1000;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) _objectNames.Add(reader.GetString(0));
                    }
                }


                // }
                //else
                //{
                //    return _objectNames;
                //}
            }
            catch (SqlException exc)
            {
                MessageBox.Show(exc.ToString());
            }

            return _objectNames;
        }
        //S01N25020U001N01D0035N01PAI____PI00
        private TimeSeriesData _timeSeriesData = new TimeSeriesData();
        public TimeSeriesData TimeSeriesData
        {
            get => _timeSeriesData;
        }
        public async Task<TimeSeriesData> LoadObjectDataAsync(string objectName)
        {
            SelectedObject = objectName;
            OnSelectedObjectChanged();
            try
            {
                var sql = " SELECT DBTimeStamp, Value FROM SEICDatabase.dbo.SEICEVENTHISTORY WHERE Ev_Info = @param;";
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.Parameters.AddWithValue("@param", objectName);
                    command.CommandTimeout = 1000;
                    var date = new List<DateTime>();
                    var values = new List<float>();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {

                            _timeSeriesData.Points.Add(new Point(reader.GetDateTime(0), reader.GetFloat(1)));

                            // values.Add(reader.GetFloat(1));
                        }
                    }
                }
            }
            catch (SqlException exc)
            {
                MessageBox.Show(exc.ToString());
            }
            //_timeSeriesData.SeriesType = 0;
            OnDataObjectLoaded?.Invoke();
            return _timeSeriesData;

        }
    }
}
