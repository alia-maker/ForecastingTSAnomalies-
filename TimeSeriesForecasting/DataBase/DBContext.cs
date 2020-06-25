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

        private static readonly string filePath = Path.Combine(Directory.GetCurrentDirectory(),  "ConnectionInfo.json");
        public string SelectedObject { get; set; } = "S01N95169U001N01D0035N01PAI____PI00";

        public event Action ONConnectionStatusChanged;

        public event Action OnSelectedObjectChanged;

        public event Action OnDataObjectLoaded;

        public event Action OnObjectNamesLoaded;
        public event Action OnRequestExecution;
        public event Action OnRequestCompleted;

        IFileWorker _fileWorker;

        public void CheckConnection(DBConnectionInfo connectioninfo)
        {
            var result = _openDBConnectionAsync(connectioninfo);
            if (result)
            {
                MessageBox.Show("Соединение успешно");
                _connection.Close();
            }
            else
                MessageBox.Show("Соединение не установлено");
        }
        public bool PlotBrawser { get; set; }
        public float ScalingFactorHoltWinters { get; set; } = (float)2.1;
        public float ScalingFactorXGBoost { get; set; } = (float)1.5;

        public bool CreateConnection(DBConnectionInfo connectioninfo)
        {
            _fileWorker.Save(connectioninfo, filePath);
            var result = _openDBConnectionAsync(connectioninfo);
            if (!result)
            {
                ConnectionStatus = "Соединение с базой данных не установлено";
                MessageBox.Show(ConnectionStatus);
            }
            else
            {
                ConnectionStatus = "Соединение c базой данных установлено";
                _connectionInfo = connectioninfo;
            }

            ONConnectionStatusChanged();
            return result;
        }


        public DBContext(IFileWorker fileWorker)
        {
            _fileWorker = fileWorker;
            //попытка восстановить последнее соединение
            _tryLastConnect();
        }

        private void _tryLastConnect()
        {
            try
            {
                var connectPath = filePath;
                if (File.Exists(connectPath))
                {
                    _connectionInfo = _fileWorker.Read<DBConnectionInfo>(filePath, "");
                    var result = _openDBConnectionAsync(_connectionInfo);
                    var result1 = false;
                    if (result) 
                        result1 = CheckSelectedFields(_connectionInfo.Tablename, 
                            _connectionInfo.ColumnTag, _connectionInfo.ColumnTimestamp, _connectionInfo.ColumnValue);
                    if (result1)
                        ConnectionStatus = " Соединение с базой данных установлено";
                    else ConnectionStatus = "Установите параметры соединения с базой данных";
                }
                else
                    _connectionStatus = "Установите параметры соединения с базой данных";

            }
            catch (Exception e)
            {
                _connectionStatus = "Установите параметры соединения с базой данных";
            }
            ONConnectionStatusChanged?.Invoke();
        }




      
        private string _connectionStatus;
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => _connectionStatus = value;


        }

        public bool IntegratedSecurity { get; set; } = true;
        public List<string> TableNames = new List<string>();

        private bool _openDBConnectionAsync(DBConnectionInfo connectionInfo)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = connectionInfo.DataSource,
                UserID = connectionInfo.UserID,
                IntegratedSecurity = IntegratedSecurity,
                Password = connectionInfo.Password,
                InitialCatalog = connectionInfo.InitialCatalog,
                MultipleActiveResultSets = true,
            };
            _connection = new SqlConnection(builder.ConnectionString);
            try
            {
                _connection.Open();
               // var sql = "SELECT name FROM " + connectionInfo.InitialCatalog + ".sys.tables";
               var sql = "SELECT DISTINCT TABLE_NAME FROM " +
                "SEICDatabase.INFORMATION_SCHEMA.COLUMNS as col WHERE " +
                "(exists" + 
                "(SELECT DATA_TYPE FROM " +
                "SEICDatabase.INFORMATION_SCHEMA.COLUMNS where " +
                "(DATA_TYPE = 'varchar' " +
                "or DATA_TYPE = 'char' " +
                "or DATA_TYPE = 'text' " +
                "or DATA_TYPE = 'nchar' " +
                "or DATA_TYPE = 'nvarchar' " +
                "or DATA_TYPE = 'ntext') " +
                "and TABLE_NAME = col.TABLE_NAME) " +
                "and exists " +
                "(SELECT COLUMN_NAME, DATA_TYPE  FROM " +
                "SEICDatabase.INFORMATION_SCHEMA.COLUMNS WHERE " +
                "(DATA_TYPE = 'float' " +
                "or DATA_TYPE = 'real' " +
                "or DATA_TYPE = 'int' " +
                "or DATA_TYPE = 'bigint' " +
                "or DATA_TYPE = 'smallint') " +
                "and TABLE_NAME = col.TABLE_NAME) " +
                "and exists " +
                "(SELECT COLUMN_NAME, DATA_TYPE  FROM " +
                "SEICDatabase.INFORMATION_SCHEMA.COLUMNS WHERE " +

                "(DATA_TYPE = 'datetime' " +
                "or DATA_TYPE = 'datetime2' " +
                "or DATA_TYPE = 'time' " +
                "or DATA_TYPE = 'datetimeoffset' " +
                "or DATA_TYPE = 'smalldatetime') " +
                "and TABLE_NAME = col.TABLE_NAME))";
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.CommandTimeout = 1000;
                    TableNames.Clear();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) TableNames.Add(reader.GetString(0));
                    }
                    return true;
                }
            }
            catch (Exception )
            {
                return false;
            }

        }

        public bool CheckSelectedFields(string tablename  ,string ctag, string ctimestamp, string cvalue)
        {
            try
            {
                
                //_connection.Open();

                var sql = " SELECT TOP (2) " +  ctag + ", " + ctimestamp + ", " + cvalue +  
                    " FROM " + _connectionInfo.InitialCatalog + ".dbo." + tablename;
                using (var command = new SqlCommand(sql, _connection))
                {
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) { }
                    }
                    _connectionInfo.ColumnTag = ctag;
                    _connectionInfo.ColumnTimestamp = ctimestamp;
                    _connectionInfo.ColumnValue = cvalue;
                    _fileWorker.Save(_connectionInfo, filePath);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }



        public List<string> ColumnNames { get; set; } = new List<string>();

        public bool LoadColumnTypes(string tablename)
        {
            _connectionInfo.Tablename = tablename;
            try
            {
                ColumnNames.Clear();
                var sql = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM " + _connectionInfo.InitialCatalog +
                    ".INFORMATION_SCHEMA.COLUMNS WHERE table_name='" + _connectionInfo.Tablename + "'";
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) ColumnNames.Add(reader.GetString(1));
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        private DBConnectionInfo _connectionInfo;


        public void Dispose()
        {
            _connection?.Dispose();
        }

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
                //var sql = " SELECT DISTINCT Ev_Info FROM " + _connectionInfo.InitialCatalog +  ".dbo.SEICEVENTHISTORY" +
                //          " WHERE right(Ev_Info,5) = '_PI00' ";

                var sql = " SELECT DISTINCT " + _connectionInfo.ColumnTag + " FROM " + _connectionInfo.InitialCatalog + ".dbo." + _connectionInfo.Tablename +
                          " WHERE right(" + _connectionInfo.ColumnTag +",5) = '_PI00' ";
                ObjectNames.Clear();
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.CommandTimeout = 1000;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) ObjectNames.Add(reader.GetString(0));
                    }
                }
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

        public TimeSeriesData TimeSeriesData
        {
            get;
        } = new TimeSeriesData();

        public async Task<TimeSeriesData> LoadObjectDataAsync(string objectName, DateTime start, DateTime end)
        {
            ConnectionStatus = "Загрузка данных из БД...";
            ONConnectionStatusChanged?.Invoke();

            SelectedObject = objectName;
            OnSelectedObjectChanged?.Invoke();
            try
            {
                //var sql = " SELECT DBTimeStamp, Value FROM " + _connectionInfo.InitialCatalog + ".dbo.SEICEVENTHISTORY WHERE Ev_Info = @param" +
                //    " and DBTimeStamp > @start and DBTimeStamp < @end";

                var sql = " SELECT " + _connectionInfo.ColumnTimestamp + ", " + _connectionInfo.ColumnValue + " FROM " + 
                    _connectionInfo.InitialCatalog + ".dbo." + _connectionInfo.Tablename + " WHERE " + _connectionInfo.ColumnTag + " = @param" +
                   " and " + _connectionInfo.ColumnTimestamp + " > @start and " + _connectionInfo.ColumnTimestamp + " < @end";
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.Parameters.AddWithValue("@param", objectName);
                    command.Parameters.AddWithValue("@start", start);
                    command.Parameters.AddWithValue("@end", end);
                    command.CommandTimeout = 1000;
                    TimeSeriesData.Points.Clear();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        TimeSeriesData.Name = objectName;
                        while (reader.Read())
                        {
                            TimeSeriesData.Points.Add(new Point(reader.GetDateTime(0), reader.GetFloat(1)));
                        }
                    }
                }
            }
            catch (SqlException exc)
            {
//                MessageBox.Show("При загрузке данных произошла ошибка");
                MessageBox.Show(exc.ToString());

            }
            OnDataObjectLoaded?.Invoke();
            ConnectionStatus = "Загрузка данных завершена";
            ONConnectionStatusChanged?.Invoke();
            return TimeSeriesData;

        }

        public MyClassDto PlotData {get; set;}
    }
}
