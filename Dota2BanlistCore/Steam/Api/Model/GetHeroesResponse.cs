using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Steam.Api.Model
{
    public class GetHeroesResponse
    {
        [JsonProperty("heroes")]
        public List<Hero> Heroes { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }

        public class Hero
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("localized_name")]
            public string LocalizedName { get; set; }
        }
    }
}
