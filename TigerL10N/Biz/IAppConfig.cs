using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerL10N.Biz
{
    public interface IAppConfig
    {
        string AuthClientId { get; }

        string AuthClientSecret { get; }

        #region Process Extract Folder Position
        string LastOpenFile { get; set; }
        string LastOpenFolder { get; set; }
        string LastSaveFolder { get; set; }
        string LastSaveFile { get; set; }

        string LastL18NFile { get; set; }
        string LastLSolutionFile { get; set; }
        
        #endregion

    }
}
