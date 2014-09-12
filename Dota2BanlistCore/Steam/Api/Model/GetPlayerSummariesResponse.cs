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
using Steam.JsonConverters;
using Newtonsoft.Json;

namespace Steam.Api.Model
{
    public class GetPlayerSummariesResponse
    {
        [JsonProperty("players")]
        public IList<PlayerSummary> Players { get; set; }
    }

    //For complete data see: https://developer.valvesoftware.com/wiki/Steam_Web_API#GetPlayerSummaries_.28v0002.29
    //Only added a few things that are needed.

    public class PlayerSummary
    {
        [JsonProperty("steamid"), JsonConverter(typeof(Steam64ToSteamId))]
        public SteamId SteamId { get; set; }

        [JsonProperty("communityvisibilitystate")]
        public PlayerSummaryVisibility CommunityVisibilityState { get; set; }

        [JsonProperty("profilestate")]
        public int ProfileState { get; set; }

        [JsonProperty("personaname")]
        public string PersonaName { get; set; }

        [JsonProperty("lastlogoff"), JsonConverter(typeof(IntegerToDateTime))]
        public DateTime? LastLogoff { get; set; }

        [JsonProperty("commentpermission")]
        public int CommentPermission { get; set; }

        [JsonProperty("profileurl")]
        public string ProfileUrl { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("avatarmedium")]
        public string AvatarMedium { get; set; }

        [JsonProperty("avatarfull")]
        public string AvatarFull { get; set; }

        [JsonProperty("personastate")]
        public PlayerSummaryPersonaState PersonaState { get; set; }

        [JsonProperty("primaryclanid")]
        public string PrimaryClanId { get; set; }

        [JsonProperty("timecreated"), JsonConverter(typeof(IntegerToDateTime))]
        public DateTime? TimeCreated { get; set; }

        [JsonProperty("loccountrycode")]
        public string LocCountryCode { get; set; }

        [JsonProperty("locstatecode")]
        public string LocStateCode { get; set; }

        public bool IsPublic
        {
            get
            {
                return CommunityVisibilityState != PlayerSummaryVisibility.Private &&
                       CommunityVisibilityState != PlayerSummaryVisibility.FriendsOnly;
            }
        }

    }

    public enum PlayerSummaryVisibility
    {
        Private = 1,
        FriendsOnly = 2,
        FriendsOfFriends = 3, // Currently this value is also set for "Public" and "Users Only" profiles
        UsersOnly = 4,
        Public = 5,
    }

    public enum PlayerSummaryPersonaState
    {
        Offline = 0, // (Also set when the profile is Private)
        Online = 1,
        Busy = 2,
        Away = 3,
        Snooze = 4,
        LookingToTrade = 5,
        LookingToPlay = 6,
    }

}
