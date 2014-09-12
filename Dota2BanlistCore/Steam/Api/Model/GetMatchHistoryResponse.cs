using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Steam.JsonConverters;

namespace Steam.Api.Model
{
    public class MatchHistoryMatch
    {
        [JsonProperty("match_id")]
        public long MatchId { get; set; }
        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }
        [JsonProperty("start_time"), JsonConverter(typeof(IntegerToDateTime))]
        public DateTime StartTime { get; set; }
        [JsonProperty("lobby_type")]
        public MatchLobbyType LobbyType { get; set; }
        [JsonProperty("players")]
        public List<MatchHistoryPlayer> Players { get; set; }
    }

    public class MatchHistoryPlayer
    {
        [JsonProperty("account_id"), JsonConverter(typeof(AccountIdToSteamId))]
        public SteamId AccountId { get; set; }
        [JsonProperty("player_slot")]
        public int EncodedPlayerSlot { get; set; }
        [JsonProperty("hero_id")]
        public int HeroId { get; set; }

        public int Slot
        {
            get { return EncodedPlayerSlot & 0x7; }
        }
        public MatchHistoryTeams Team
        {
            get { return (EncodedPlayerSlot & 0x80) == 0 ? MatchHistoryTeams.Radiant : MatchHistoryTeams.Dire; }
        }
    }

    public class GetMatchHistoryResponse
    {
        [JsonProperty("status")]
        public MatchHistoryStatus Status { get; set; }
        [JsonProperty("total_results")]
        public int TotalResults { get; set; }
        [JsonProperty("results_remaining")]
        public int ResultsRemaining { get; set; }
        [JsonProperty("num_results")]
        public int NumResults { get; set; }
        [JsonProperty("matches")]
        public List<MatchHistoryMatch> Matches { get; set; }

    }

    public enum MatchHistoryStatus
    {
        Success = 1,
        Private = 15
    }
    public enum MatchLobbyType
    {
        Invalid = -1,
        PublicMatchmaking = 0,
        Practise = 1,
        Tournament = 2,
        Tutorial = 3,
        CoopWithBots = 4,
        TeamMatch = 5,
        SoloQueue = 6,
    }

    public enum MatchHistoryTeams
    {
        Radiant,
        Dire
    }
}
