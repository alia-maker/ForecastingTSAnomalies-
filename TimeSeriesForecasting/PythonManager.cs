using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;
using System.Text.Json;
using System.Windows;
using TimeSeriesForecasting.HelpersLibrary;

namespace TimeSeriesForecasting
{
    public class PythonManager : IDisposable
    {
        private ZSocket _requester;

        public PythonManager()
        {
            try
            {
                _requester = new ZSocket(ZSocketType.REQ);
                _requester.Connect("tcp://127.0.0.1:5555");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public void Send<T>(T obj)
        {
            JsonFileWorker file_Creater = new JsonFileWorker();
            file_Creater.Save<T>(obj, "file2python.json");
            string json = JsonSerializer.Serialize(obj);
            _requester.SendFrame(new ZFrame(json));
        }

        public T Receive<T>()
        {
            using (var reply = _requester.ReceiveFrame())
            {
                var json = reply.ReadString();
                var obj = JsonSerializer.Deserialize<T>(json);
                return obj;
            }
        }
        public void Dispose()
        {
            _requester.Dispose();
        }

    }
}
