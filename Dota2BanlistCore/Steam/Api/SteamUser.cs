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
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steam.Api.Model;

namespace Steam.Api
{
    public class SteamUserApi : BaseServiceApi
    {
        public SteamUserApi(string key) 
            : base(key)
        {
        }

        protected override string ServiceName
        {
            get { return "ISteamUser"; }
        }

        public GetPlayerSummariesResponse GetPlayerSummaries(params SteamId[] steamIds)
        {
            using (var wc = new WebClient())
            {
                var url = GetCallUrl("GetPlayerSummaries", "v0002", Tuple.Create("steamids", string.Join(",", steamIds.Select(s => s.ToSteamId64()))));
                var data = wc.DownloadString(url);
                var obj = JsonConvert.DeserializeObject<JObject>(data);
                var resp = JsonConvert.DeserializeObject<GetPlayerSummariesResponse>(obj["response"].ToString());
                return resp;
            }
        }

        public GetFriendListResponse GetFriendList(SteamId steamId, SteamUserRelationship relationship = SteamUserRelationship.friend)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var url = GetCallUrl("GetFriendList", "v0001", Tuple.Create("steamid", steamId.ToSteamId64().ToString()), Tuple.Create("relationship", relationship.ToString()));
                    var data = wc.DownloadString(url);
                    var obj = JsonConvert.DeserializeObject<JObject>(data);
                    var resp = JsonConvert.DeserializeObject<GetFriendListResponse>(obj["friendslist"].ToString());
                    return resp;
                }
            }
            catch (WebException we)
            {
                if (we.Response != null && ((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Unauthorized)
                    return new GetFriendListResponse();
                throw;
            }
        }
        public GetPlayerBansResponse GetPlayerBans(params SteamId[] steamIds)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var url = GetCallUrl("GetPlayerBans", "v0001", Tuple.Create("steamids", string.Join(",", steamIds.Select(s => s.ToSteamId64()))));
                    var data = wc.DownloadString(url);
                    var resp = JsonConvert.DeserializeObject<GetPlayerBansResponse>(data);
                    return resp;
                }
            }
            catch (WebException we)
            {
                if (we.Response != null && ((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Unauthorized)
                    return new GetPlayerBansResponse();
                throw;
            }
        }

        public enum SteamUserRelationship
        {
            all,
            friend
        }
    }
}
