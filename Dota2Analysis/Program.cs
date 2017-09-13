using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Steam;
using Steam.Api;
using Steam.Api.Model;
using Util;

namespace Dota2Analysis
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var steamId = SteamId.ParseEncodedId(args[0]);
            var steamKey = args[1];

            /*var api = new DotaMatchesApi(steamKey);
            var steamApi = new SteamUserApi(steamKey);
            var dbPath = Path.Combine(Environment.CurrentDirectory, "test.db");
            var db = new DbMaster(new SQLiteConnection("Data Source=" + dbPath));
            db.Initialize();

            var matches = db.GetAllMatches();
            foreach (var match in matches)
            {
                var steamIds = match.Players.Where(p => p.AccountId != null && !p.IsAnonymous).Select(p => p.AccountId).ToArray();
                var summaries = steamApi.GetPlayerSummaries(steamIds);
                var playersAndDota = summaries.Players.Select(p => {
                    var psa = new PlayerServiceApi(steamKey);
                    var dota = psa.GetOwnedGames(p.SteamId, 570);
                    var game = dota.Games != null ? dota.Games.FirstOrDefault(g => g.AppId == 570) : null;
                    Thread.Sleep(1000);
                    return Tuple.Create(p, game);
                }).ToList();

                db.InsertPlayers(playersAndDota);
                Console.WriteLine("Processed match " + match.MatchId + ". " + summaries.Players.Count + " players added");
                Thread.Sleep(1000);
            }*/


            //var matches = db.GetAllMatches();

            //var winRatesByDay = new dynamic[7];
            //for (int i = 0; i < winRatesByDay.Length; i++)
            //{
            //    var matchesOnDay = matches.Where(m => i == (int)m.StartTime.DayOfWeek && GetMyTeam(m, steamId).HasValue).ToList();
            //    var wins = matchesOnDay.Count(m => m.TeamWin == GetMyTeam(m, steamId).Value);
            //    var winRate = wins / (double)matchesOnDay.Count;
            //    winRatesByDay[i] = new { winRate, count = matchesOnDay.Count };
            //}

            //var winRatesByHour = new dynamic[24];
            //for (int i = 0; i < winRatesByHour.Length; i++)
            //{
            //    var matchesPerHour = matches.Where(m => i == m.StartTime.Hour && GetMyTeam(m, steamId).HasValue).ToList();
            //    var winRate = matchesPerHour.Count(m => m.TeamWin == GetMyTeam(m, steamId).Value) / (double)matchesPerHour.Count;
            //    winRatesByHour[i] = new { winRate, count = matchesPerHour.Count };
            //}

            //Clipboard.SetText(string.Join("\r\n", winRatesByDay.Select((wr, i) => string.Format("{0},{1},{2}", (DayOfWeek)i, wr.winRate, wr.count))));

            //return;



            //var heroes = new DotaEconApi(steamKey).GetHeroes().Heroes;
            //db.InsertHeroes(heroes);

            //foreach (var hero in heroes)
            //{
            //    var history = api.GetMatchHistory(steamId, heroId: hero.Id);
            //    do
            //    {
            //        foreach (var match in history.Matches)
            //        {
            //            var details = api.GetMatchDetails(match.MatchId);
            //            db.InsertMatch(details);
            //            Thread.Sleep(1000);
            //        }

            //        if (history.ResultsRemaining < 1)
            //            break;
            //        history = api.GetMatchHistory(steamId, startingAtMatch: history.Matches.Last().MatchId.ToString(), heroId: hero.Id);

            //    } while (history.Matches.Count > 1);
            //}
        }

        static MatchHistoryTeams? GetMyTeam(GetMatchDetailsResponse match, SteamId me)
        {
            var player = match.Players.FirstOrDefault(p => p.AccountId.ToEncodedId() == me.ToEncodedId());
            return player != null ? (MatchHistoryTeams?)player.Team : null;
        }
    }

    public class DbMaster
    {
        readonly IDbConnection m_Connection;

        public DbMaster(IDbConnection connection)
        {
            m_Connection = connection;
        }

        public void Initialize()
        {
            m_Connection.Query(CreateTablesQuery);
        }

        public void InsertHeroes(List<GetHeroesResponse.Hero> heroes)
        {
            if (heroes.Count > 200)
                throw new ArgumentException("Heroes cannot exceed 200. Batch the inserts.");
            var argString = string.Join(",", heroes.Select((h, i) => "(" + string.Join(", ", Enumerable.Range(i * 3, 3).Select(n => "@" + n)) + ")"));
            var query = @"INSERT OR REPLACE INTO heroes (id, name, localizedName) VALUES " + argString;
            var heroesFlat = heroes.Select(h => new object[] { h.Id, h.Name, h.LocalizedName }).SelectMany(o => o).ToArray();
            m_Connection.Query(query, heroesFlat);
        }

        public void InsertMatch(GetMatchDetailsResponse match)
        {
            var query = @"INSERT OR REPLACE INTO rawMatches (matchId, matchSeqNum, matchStartTime, matchJson) VALUES (@0, @1, @2, @3)";
            m_Connection.Query(query, match.MatchId, match.MatchSeqNum, DateHelper.DateTimeToUnixTimestamp(match.StartTime), JsonConvert.SerializeObject(match));
        }

        public void InsertPlayers(List<Tuple<PlayerSummary, OwnedGame>> players)
        {
            if (players.Count > 200)
                throw new ArgumentException("Players cannot exceed 200. Batch the inserts.");
            var argString = string.Join(",", players.Select((h, i) => "(" + string.Join(", ", Enumerable.Range(i * 5, 5).Select(n => "@" + n)) + ")"));
            var query = @"INSERT OR REPLACE INTO players (steamId, isPublic, timeCreated, hoursPlayed, rawJson) VALUES " + argString;
            var heroesFlat = players.Select(h => new object[] { h.Item1.SteamId.ToSteamId64(), h.Item1.IsPublic, h.Item1.TimeCreated, h.Item2 != null ? (int?)h.Item2.PlaytimeForever : null, JsonConvert.SerializeObject(h) }).SelectMany(o => o).ToArray();
            m_Connection.Query(query, heroesFlat);
        }

        public List<GetMatchDetailsResponse> GetAllMatches()
        {
            var list = new List<GetMatchDetailsResponse>();
            var query = @"SELECT matchJson FROM rawMatches";
            using (var reader = m_Connection.QueryReader(query))
            {
                while (reader.Read())
                    list.Add(JsonConvert.DeserializeObject<GetMatchDetailsResponse>(reader.Get<string>("matchJson")));
            }
            return list;
        }

        const string CreateTablesQuery = @"
CREATE TABLE IF NOT EXISTS heroes ( 
    id            INTEGER PRIMARY KEY,
    name          TEXT,
    localizedName TEXT 
);
CREATE TABLE IF NOT EXISTS rawMatches ( 
    matchId     INTEGER PRIMARY KEY,
    matchSeqNum INTEGER,
    matchStartTime INTEGER,
    matchJson   TEXT 
);
CREATE TABLE IF NOT EXISTS players ( 
    steamId     INTEGER PRIMARY KEY,
    isPublic    BOOLEAN,
    timeCreated INTEGER,
    hoursPlayed INTEGER,
    rawJson     TEXT 
);

";
    }
}
