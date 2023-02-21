using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TigerL10N.Biz;
using Config.Net;

namespace TigerL10N.Service
{
    public static class AppConfigService
    {
        public static IAppConfig Settings;
        private const string ConfigFile = "app.json";

        static AppConfigService()
        {
            Settings = new ConfigurationBuilder<IAppConfig>()
                //.UseAppConfig() // readonly
                //.UseInMemoryDictionary() // volatile
                //.UseEnvironmentVariables() // runtime
                //.UseIniFile(filePath)
                .UseJsonFile(ConfigFile)
                .Build();
        }

        private static void Init_Setting()
        {

        }

    }
}
