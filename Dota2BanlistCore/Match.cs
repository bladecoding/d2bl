using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Dota2BanlistCore
{
    public class Match
    {
        public DateTime Date { get; set; }
        public IPEndPoint IpAddress { get; set; }
        public string Gamemode { get; set; }
        public IList<SteamId> SteamIds { get; set; }

    }
}
