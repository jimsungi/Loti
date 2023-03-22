using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerL10N.Biz
{
    public class LogEntry : BindableBase
    {
        public LogEntry(string log)
        {
            DateTime = System.DateTime.Now;
            Message = log;
        }
        public DateTime DateTime { get; set; }

        public int Index { get; set; }

        private string _message = "";
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}
