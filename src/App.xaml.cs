using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using TigerL10N.ViewModels;
using TigerL10N.Views;

namespace TigerL10N
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            var w = Container.Resolve<Views.MainWindow>();
            return w;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // pretend to register a service
            //containerRegistry.Register<Services.ISampleService, Services.DbSampleService>();
            // register other needed services here

            // Register Application Range Command
            //containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
            //containerRegistry.RegisterSingleton<IMahDialogService, MahDialogService>();
            //containerRegistry.RegisterSingleton<IMahThemeService, MahThemeService>();
            //containerRegistry.RegisterSingleton<IUserSettingService, UserSettingService>();

            // Register Dialog
            containerRegistry.RegisterDialog<NewProjectDlg, NewProjectDlgViewModel>("NewProject");
        }
    }
}
