using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steam;
using Steam.Api;
using Steam.Api.Model;
using Util;

namespace Steam.Api
{
    public class DotaMatchesApi : BaseServiceApi
    {
        public DotaMatchesApi(string key) : base(key)
        {
        }


        protected override string ServiceName
        {
            get { return "IDOTA2Match_570"; }
        }

        public GetMatchHistoryResponse GetMatchHistory(SteamId steamId, string startingAtMatch = null, int? heroId = null)
        {
            using (var wc = new WebClient())
            {
                var startingAtTup = startingAtMatch != null ? Tuple.Create("start_at_match_id", startingAtMatch) : null;
                var heroIdTup = heroId != null ? Tuple.Create("hero_id", heroId.Value.ToString()) : null;
                var url = GetCallUrl("GetMatchHistory", "v0001", Tuple.Create("account_id", steamId.ToEncodedIdString()), startingAtTup, heroIdTup);
                var data = wc.DownloadString(url);
                var obj = JsonConvert.DeserializeObject<JObject>(data);
                var resp = JsonConvert.DeserializeObject<GetMatchHistoryResponse>(obj["result"].ToString());
                return resp;
            }
        }
        public GetMatchDetailsResponse GetMatchDetails(long matchId)
        {
            using (var wc = new WebClient())
            {
                var url = GetCallUrl("GetMatchDetails", "v0001", Tuple.Create("match_id", matchId.ToString()));
                var data = wc.DownloadString(url);
                var obj = JsonConvert.DeserializeObject<JObject>(data);
                var resp = JsonConvert.DeserializeObject<GetMatchDetailsResponse>(obj["result"].ToString());
                return resp;
            }
        }

    }
}
