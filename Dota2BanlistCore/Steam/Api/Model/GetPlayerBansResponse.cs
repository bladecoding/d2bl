/*
This file is part of Dota2Banlist.

Dota2Banlist is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Dota2Banlist is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Dota2Banlist.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Steam.JsonConverters;

namespace Steam.Api.Model
{
    public class GetPlayerBansResponse
    {
        [JsonProperty("players")]
        public IList<PlayerBan> Players { get; set; }
    }

    public class PlayerBan
    {
        [JsonProperty("SteamId"), JsonConverter(typeof(Steam64ToSteamId))]
        public SteamId SteamId { get; set; }

        [JsonProperty("CommunityBanned")]
        public bool CommunityBanned { get; set; }

        [JsonProperty("VACBanned")]
        public bool VACBanned { get; set; }

        [JsonProperty("NumberOfVACBans")]
        public int NumberOfVACBans { get; set; }

        [JsonProperty("DaysSinceLastBan")]
        public int DaysSinceLastBan { get; set; }

        [JsonProperty("NumberOfGameBans")]
        public int NumberOfGameBans { get; set; }

        [JsonProperty("EconomyBan")]
        public string EconomyBan { get; set; }


    }
}
