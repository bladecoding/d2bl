using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Steam;

namespace Dota2BanlistCore
{
    public class Match
    {
        public DateTime Date { get; set; }
        public IPEndPoint IpAddress { get; set; }
        public Int64 LobbyId { get; set; }
        public string Gamemode { get; set; }
        public IList<SteamId> SteamIds { get; set; }
        public IList<SteamId> PartySteamIds { get; set; }

    }
}
