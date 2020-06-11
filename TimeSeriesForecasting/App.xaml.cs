using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TimeSeriesForecasting.DataBase;
using TimeSeriesForecasting.HelpersLibrary;
using TimeSeriesForecasting.ModelBuilding;
using TimeSeriesForecasting.ViewModels;

namespace TimeSeriesForecasting
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            // building
            var container = services.BuildServiceProvider();

            // show main window
            var viewManager = container.GetService<ViewManager>();

            var (_, MainWindow) = viewManager.GetWindow<MainWindowViewModel, MainWindow>();
            MainWindow.ShowDialog();
            Shutdown(0);
        }
        private void ConfigureServices(IServiceCollection services)
        {
            // viewModels
            services.AddTransient<DBConnectionWindowViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ObjectSelectionWindowViewModel>();
            services.AddTransient<ModelParamsSelectorVM<HoltWintersModelParams>>();
            services.AddTransient<ModelParamsSelectorVM<XGBoostModelParams>>();

            services.AddTransient<HoltWintersWindowViewModel>();
            services.AddTransient<XGBoostWindowViewModel>();
            services.AddSingleton<HoltWintersModel>();
            services.AddSingleton<XGBoostModel>();
            services.AddTransient<IModelParamsReader, ModelParamsReader>();
            services.AddTransient<IFileWorker, JsonFileWorker>();
            services.AddTransient<IModel, HoltWintersModel>();
            services.AddTransient<IModel, XGBoostModel>();
            // services
            //services.AddTransient<IMyCalculator, MyCalculator>();
            services.AddSingleton<ViewManager>(); // wanna have only one manager
            services.AddSingleton<DBContext>();
            services.AddSingleton<PythonManager>();
            //// read external json configurations
            //var config = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //    .Build();

            //// strongly-typed setting class
            //services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));

            //// setting up logging
            //services.AddLogging(builder =>
            //{
            //    var conf = new NLogLoggingConfiguration(config.GetSection(nameof(NLog)));
            //    builder.AddNLog(conf);
            //});
        }
    }
}
