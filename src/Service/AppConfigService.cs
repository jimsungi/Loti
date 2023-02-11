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
        public static IAppConfig? Settings=null;
        private const string ConfigFile = "app.json";

        static AppConfigService()
        {
            Init_Setting();
        }

        private static void Init_Setting()
        {
            Settings = new ConfigurationBuilder<IAppConfig>()
                //.UseAppConfig() // readonly
                                //.UseInMemoryDictionary() // volatile
                                //.UseEnvironmentVariables() // runtime
                                //.UseIniFile(filePath)
                .UseJsonFile(ConfigFile)
                .Build();
        }

        public static void ClearMemory()
        {
            Settings = null;
        }
    }
}
