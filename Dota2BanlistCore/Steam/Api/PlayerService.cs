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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steam.Api.Model;

namespace Steam.Api
{
    public class PlayerServiceApi : BaseServiceApi
    {
        public PlayerServiceApi(string key)
            : base(key)
        {
        }

        protected override string ServiceName
        {
            get { return "IPlayerService"; }
        }

        public GetOwnedGamesResponse GetOwnedGames(SteamId steamId, params int[] appids)
        {
            using (var wc = new WebClient())
            {
                var req = new GetOwnedGamesRequest
                {
                    SteamId = steamId,
                    AppIdsFilter = appids != null ? appids.ToList() : null, //appids filter does nothing?
                    IncludePlayedFreeGames = true
                };

                var url = GetCallUrl("GetOwnedGames", "v0001", Tuple.Create("input_json", JsonConvert.SerializeObject(req)));

                var data = wc.DownloadString(url);
                var obj = JsonConvert.DeserializeObject<JObject>(data);
                var resp = JsonConvert.DeserializeObject<GetOwnedGamesResponse>(obj["response"].ToString());
                return resp;
            }
        }
    }
}
