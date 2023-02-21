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
        public static StringParser CreateParser()
        {
            StringParser instance = new();
            return instance;
        }
    }
}
