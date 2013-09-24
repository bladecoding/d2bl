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
    public class GetFriendListResponse
    {
        [JsonProperty("friends")]
        public IList<SteamFriend> Friends {get;set;} 
    }

    public class SteamFriend
    {
        [JsonProperty("steamid"), JsonConverter(typeof(Steam64ToSteamId))]
        public SteamId SteamId { get; set; }

        [JsonProperty("relationship")]
        public string Relationship { get; set; }

        [JsonProperty("friend_since"), JsonConverter(typeof(IntegerToDateTime))]
        public DateTime FriendSince { get; set; }
    }
}
