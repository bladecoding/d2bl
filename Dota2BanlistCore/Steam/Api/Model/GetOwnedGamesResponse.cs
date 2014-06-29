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
    public class GetOwnedGamesResponse
    {
        [JsonProperty("game_count")]
        public int GameCount { get; set; }
        [JsonProperty("games")]
        public IList<OwnedGame> Games { get; set; }
    }
    public class OwnedGame
    {
        //appid Unique identifier for the game
        //name The name of the game
        //playtime_2weeks The total number of minutes played in the last 2 weeks
        //playtime_forever The total number of minutes played "on record", since Steam began tracking total playtime in early 2009. 
        [JsonProperty("appid")]
        public int AppId { get; set; }
        [JsonProperty("name")]
        public int Name { get; set; }
        [JsonProperty("playtime_2weeks")]
        public int Playtime2Weeks { get; set; }
        [JsonProperty("playtime_forever")]
        public int PlaytimeForever { get; set; }
    }
    public class GetOwnedGamesRequest
    {
        [JsonProperty("steamid", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(Steam64ToSteamId))]
        public SteamId SteamId { get; set; }

        [JsonProperty("appids_filter", NullValueHandling = NullValueHandling.Ignore)]
        public IList<int> AppIdsFilter { get; set; }

        [JsonProperty("include_played_free_games", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncludePlayedFreeGames { get; set; }

        [JsonProperty("include_appinfo", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncludeAppinfo { get; set; }


    }

}
