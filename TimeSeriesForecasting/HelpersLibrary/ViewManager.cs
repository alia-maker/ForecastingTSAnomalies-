using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeSeriesForecasting.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace TimeSeriesForecasting.HelpersLibrary
{
    public class ViewManager
    {
        private readonly IServiceProvider _container;

        public ViewManager(IServiceProvider container)
        {
            this._container = container;
        }

        public (TVm, TWindow) GetWindow<TVm, TWindow>()
            where TVm : ViewModel
            where TWindow : Window, new()
        {
            var vm = _container.GetService<TVm>();
            var view = new TWindow
            {
                DataContext = vm,
            };

            return (vm, view);
        }
    }
}
