using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerL10N.Service
{
    public enum LogType
    {
        Dev,Warning,Information,Error
    }
    public delegate int LogDelegate(LogType logType, DateTime logTime, string title, string message, object? option);

    public class LogService
    {
    }
}
