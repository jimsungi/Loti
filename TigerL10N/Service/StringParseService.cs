using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TigerL10N.Biz;

namespace TigerL10N.Service
{

    public class StringParseService
    {
        public static StringParserOrg CreateParser()
        {
            StringParserOrg instance = new();
            return instance;
        }
        public static StringParser CreateParser2()
        {
            StringParser instance = new();
            return instance;
        }
    }
}
