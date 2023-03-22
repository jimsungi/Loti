using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerL10N.Biz
{
    public class CollapsibleLogEntry : LogEntry
    {
        public CollapsibleLogEntry(string message) : base(message) 
        {
        }
        public List<LogEntry> Contents { get; set; }
    }
}
