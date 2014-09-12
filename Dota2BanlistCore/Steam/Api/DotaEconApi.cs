using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steam.Api.Model;

namespace Steam.Api
{
    public class DotaEconApi : BaseServiceApi
    {
        public DotaEconApi(string key) : base(key)
        {
        }

        protected override string ServiceName
        {
            get { return "IEconDOTA2_570"; }
        }

        public GetHeroesResponse GetHeroes()
        {
            using (var wc = new WebClient())
            {
                var url = GetCallUrl("GetHeroes", "v0001", Tuple.Create("language", "en"));
                var data = wc.DownloadString(url);
                var obj = JsonConvert.DeserializeObject<JObject>(data);
                var resp = JsonConvert.DeserializeObject<GetHeroesResponse>(obj["result"].ToString());
                return resp;
            }
        }
    }
}