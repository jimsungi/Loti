using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerL10N.Biz
{
    public class IgnoreOption
    {
        public List<string> IgnoreFiles = new List<string>();
        public List<string> IgnoreFolder = new List<string>();
        public IgnoreOption()
        {
            //gnoreFiles.Add(".dll");
            IgnoreFiles.Add(".obj");

            IgnoreFolder.Add("bin");
            IgnoreFolder.Add("obj");
            IgnoreFolder.Add(".ln");
            IgnoreFolder.Add(".vs");
            IgnoreFolder.Add(".git");
            IgnoreFolder.Add(".user");
        }
    }
}
