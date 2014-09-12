using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Steam.JsonConverters;


namespace Steam.Api.Model
{
    public class GetMatchDetailsResponse
    {
        [JsonProperty("players")]
        public List<MatchDetailsPlayer> Players { get; set; }


        [JsonProperty("season")]
        public string Season { get; set; }
        [JsonProperty("radiant_win")]
        public bool RadiantWin { get; set; }

        public MatchHistoryTeams TeamWin
        {
            get { return RadiantWin ? MatchHistoryTeams.Radiant : MatchHistoryTeams.Dire; }
        }

        //duration in seconds
        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("start_time"), JsonConverter(typeof(IntegerToDateTime))]
        public DateTime StartTime { get; set; }

        [JsonProperty("match_id")]
        public long MatchId { get; set; }
        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }
        [JsonProperty("lobby_type")]
        public MatchLobbyType LobbyType { get; set; }

        [JsonProperty("tower_status_radiant")]
        public int TowerStatusRadiant { get; set; }
        [JsonProperty("tower_status_dire")]
        public int TowerStatusDire { get; set; }
        [JsonProperty("barracks_status_radiant")]
        public int BarracksStatusRadiant { get; set; }
        [JsonProperty("barracks_status_dire")]
        public int BarracksStatusDire { get; set; }
        [JsonProperty("cluster")]
        public string Cluster { get; set; }
        [JsonProperty("first_blood_time")]
        public int FirstBloodTime { get; set; }
        [JsonProperty("human_players")]
        public int HumanPlayers { get; set; }
        [JsonProperty("leagueid")]
        public string Leagueid { get; set; }
        [JsonProperty("positive_votes")]
        public int PositiveVotes { get; set; }
        [JsonProperty("negative_votes")]
        public int NegativeVotes { get; set; }
        [JsonProperty("game_mode")]
        public MatchDetailsGameMode GameMode { get; set; }
        [JsonProperty("picks_bans")]
        public List<MatchDetailsPicksBans> PicksBans { get; set; }

    }

    public class MatchDetailsPicksBans
    {
        [JsonProperty("is_pick")]
        public bool IsPick { get; set; }
        [JsonProperty("hero_id")]
        public int HeroId { get; set; }
        [JsonProperty("team")]
        public int Team { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }
    }

    public enum MatchDetailsGameMode
    {
        None = 0,
        AllPick = 1,
        CaptainsMode = 2,
        RandomDraft = 3,
        SingleDraft = 4,
        AllRandom = 5,
        Intro = 6,
        Diretide = 7,
        ReverseCaptainsMode = 8,
        TheGreeviling = 9,
        Tutorial = 10,
        MidOnly = 11,
        LeastPlayed = 12,
        NewPlayerPool = 13,
        CompendiumMatchmaking = 14,
        CaptainsDraft = 16,
    }

    public class MatchDetailsPlayer
    {
        [JsonProperty("account_id"), JsonConverter(typeof(AccountIdToSteamId))]
        public SteamId AccountId { get; set; }
        [JsonProperty("player_slot")]
        public int EncodedPlayerSlot { get; set; }
        [JsonProperty("hero_id")]
        public int HeroId { get; set; }

        //Left to right, top to bottom
        [JsonProperty("item_0")]
        public int Item0 { get; set; }
        [JsonProperty("item_1")]
        public int Item1 { get; set; }
        [JsonProperty("item_2")]
        public int Item2 { get; set; }
        [JsonProperty("item_3")]
        public int Item3 { get; set; }
        [JsonProperty("item_4")]
        public int Item4 { get; set; }
        [JsonProperty("item_5")]
        public int Item5 { get; set; }
        public int[] Items
        {
            get { return new int[] { Item0, Item1, Item2, Item3, Item4, Item5 }; }
        }

        [JsonProperty("kills")]
        public int Kills { get; set; }
        [JsonProperty("deaths")]
        public int Deaths { get; set; }
        [JsonProperty("assists")]
        public int Assists { get; set; }

        //leaver_status = undocumented
        [JsonProperty("leaver_status")]
        public string LeaverStatus { get; set; }

        [JsonProperty("gold")]
        public int Gold { get; set; }
        [JsonProperty("last_hits")]
        public int LastHits { get; set; }
        [JsonProperty("denies")]
        public int Denies { get; set; }
        [JsonProperty("gold_per_min")]
        public int GoldPerMin { get; set; }
        [JsonProperty("xp_per_min")]
        public int XpPerMin { get; set; }
        [JsonProperty("gold_spent")]
        public int GoldSpent { get; set; }
        [JsonProperty("hero_damage")]
        public int HeroDamage { get; set; }
        [JsonProperty("tower_damage")]
        public int TowerDamage { get; set; }
        [JsonProperty("hero_healing")]
        public int HeroHealing { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("ability_upgrades")]
        public List<MatchDetailsAbilityUpgrades> AbilityUpgrades { get; set; }

        [JsonProperty("additional_units")]
        public List<MatchDetailsAdditionalUnits> AdditionalUnits { get; set; }


        public bool IsAnonymous
        {
            get { return AccountId.ToEncodedId() == uint.MaxValue; }
        }

        public int Slot
        {
            get { return EncodedPlayerSlot & 0x7; }
        }
        public MatchHistoryTeams Team
        {
            get { return (EncodedPlayerSlot & 0x80) == 0 ? MatchHistoryTeams.Radiant : MatchHistoryTeams.Dire; }
        }
    }

    public class MatchDetailsAdditionalUnits
    {
        [JsonProperty("unitname")]
        public string UnitName { get; set; }

        //Left to right, top to bottom
        [JsonProperty("item_0")]
        public int Item0 { get; set; }
        [JsonProperty("item_1")]
        public int Item1 { get; set; }
        [JsonProperty("item_2")]
        public int Item2 { get; set; }
        [JsonProperty("item_3")]
        public int Item3 { get; set; }
        [JsonProperty("item_4")]
        public int Item4 { get; set; }
        [JsonProperty("item_5")]
        public int Item5 { get; set; }
        public int[] Items
        {
            get { return new int[] { Item0, Item1, Item2, Item3, Item4, Item5 }; }
        }
    }

    public class MatchDetailsAbilityUpgrades
    {
        [JsonProperty("ability")]
        public int Ability { get; set; }
        [JsonProperty("time")]
        public int Time { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
    }
}
