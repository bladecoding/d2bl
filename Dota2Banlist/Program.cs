using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dota2BanlistCore;

namespace Dota2Banlist
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            new ServerLogMatchProvider(new FileInfo("E:\\Steam\\steamapps\\common\\dota 2 beta\\dota\\server_log.txt"));

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
