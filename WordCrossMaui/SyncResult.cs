using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    internal enum SyncResult
    {
        LocalUpload = 0,
        CloudDownload,
        Conflict,
        Fail
    }
}
