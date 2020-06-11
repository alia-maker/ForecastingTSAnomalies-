using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.ModelBuilding;
//using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace TimeSeriesForecasting.DataBase
{
    public enum TypeModelEnum  
    {
        HoltWinters,
        XGBoost,
    }
    public class DBContext : IDisposable
    {
        private SqlConnection _connection;

        public string SelectedObject { get; set; } = "S01N95169U001N01D0035N01PAI____PI00";

        public event Action ONConnectionStatusChanged;

        public event Action OnSelectedObjectChanged;

        public event Action OnDataObjectLoaded;

        public event Action OnObjectNamesLoaded;
        public event Action OnRequestExecution;
        public event Action OnRequestCompleted;
       // public event Action OnPlotDataChanged;

        private static readonly string _fileName = "ConnectionInfo.json";
        IFileWorker _fileWorker;

        public void CheckConnection(DBConnectionInfo _connectionInfo)
        {
            ConnectionStatus = "";
            _openDBConnection(_connectionInfo);
            _connection.Close();
            MessageBox.Show(ConnectionStatus);
        }

        public float ScalingFactorHoltWinters { get; set; } = (float)2.1;
        public float ScalingFactorXGBoost { get; set; } = (float)1.5;

        public void CreateConnection(DBConnectionInfo _connectionInfo)
        {
            _fileWorker.Save(_connectionInfo, _fileName, "");
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
                _connectionInfo = _fileWorker.Read<DBConnectionInfo>(_fileName, "");
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
                MultipleActiveResultSets = true,

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
            //_fileWorker.Save(ObjectNames, "fdsfsd");
            _connection?.Dispose();
        }

      //  private List<string> _objectNames = new List<string>();

        public List<string> ObjectNames
        {
            get;
        } = new List<string>();

        public async Task<List<string>> LoadObjectsAsync()
        {
            ConnectionStatus = "Загрузка объектов из БД...";
            ONConnectionStatusChanged?.Invoke();
            OnRequestExecution?.Invoke();
            try
            {
                var sql = " SELECT DISTINCT Ev_Info FROM SEICDatabase.dbo.SEICEVENTHISTORY" +
                          " WHERE right(Ev_Info,5) = '_PI00' ";
                ObjectNames.Clear();
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.CommandTimeout = 1000;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) ObjectNames.Add(reader.GetString(0));
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
            ConnectionStatus = "Загрузка объектов завершена";
            ONConnectionStatusChanged?.Invoke();
            OnObjectNamesLoaded?.Invoke();
            OnRequestCompleted?.Invoke();
            return ObjectNames;
        }
        //S01N25020U001N01D0035N01PAI____PI00
        //private TimeSeriesData _timeSeriesData = new TimeSeriesData();
        public TimeSeriesData TimeSeriesData
        {
            get;
        } = new TimeSeriesData();
        public async Task<TimeSeriesData> LoadObjectDataAsync(string objectName, DateTime start, DateTime end)
        {
            SelectedObject = objectName;
            OnSelectedObjectChanged();
            try
            {
                var sql = " SELECT DBTimeStamp, Value FROM SEICDatabase.dbo.SEICEVENTHISTORY WHERE Ev_Info = @param" +
                    " and DBTimeStamp > @start and DBTimeStamp < @end";

                using (var command = new SqlCommand(sql, _connection))
                {
                    command.Parameters.AddWithValue("@param", objectName);
                    command.Parameters.AddWithValue("@start", start);
                    command.Parameters.AddWithValue("@end", end);
                    command.CommandTimeout = 1000;
                    //var date = new List<DateTime>();
                    // var values = new List<float>();
                    TimeSeriesData.Points.Clear();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        TimeSeriesData.Name = objectName;
                        while (reader.Read())
                        {

                            TimeSeriesData.Points.Add(new Point(reader.GetDateTime(0), reader.GetFloat(1)));

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
            return TimeSeriesData;

        }

        public MyClassDto PlotData
        {
            get;
            set;
        }
    }
}
