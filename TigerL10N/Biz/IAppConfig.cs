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
        string LastI18NFilePath { get; set; }
        string LastStreamFilePath { get; set; }
        
        #endregion

    }
}
