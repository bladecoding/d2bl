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
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Dota2BanlistCore;
using Dota2BanlistCore.Db;
using DotaBuff;
using Ninject;
using MoreLinq;
using Steam;
using Steam.Api;
using Steam.Api.Model;
using Util;

namespace Dota2Banlist
{
    static class Program
    {
        public class MainApp
        {
        }

        [STAThread]
        static void Main()
        {
            //var kernal = new StandardKernel();
            //kernal.Bind<Dota2BanlistCore.Db.BanlistDbContext>().ToConstant(new Dota2BanlistCore.Db.BanlistDbContext());

            var steamKey = "8F2CC22E983A31E9FC3B51C84E5FF5ED";

            //Inb4 valve revokes my key for api request spam.

            var matches = new List<Match>();
            var server = new ServerLogMatchProvider(new FileInfo(@"E:\Steam\steamapps\common\dota 2 beta\dota\server_log.txt"));
            {
                server.MatchesFound += (sender, args) =>
                {
                    //if (args.Matches.Count != 1)
                        //return;

                    //Don't block the filewatcher!
                    //I feel dirty. Keep telling myself this is just for testing.
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        var api = new SteamUserApi(steamKey);
                        var summaries = api.GetPlayerSummaries(args.Matches.Last().SteamIds.ToArray());

                        var ids = args.Matches.Last().SteamIds;

                        var columns = new ColumnLines();
                        columns.Spacing = 2;
                        columns.AddLine(
                                "Index",
                                "Name",
                                "Party",
                                "Created",
                                "Hours",
                                "Country",
                                "Matches",
                                "WinRatio",
                                "LastPlayed");

                        var playerData = new List<Tuple<PlayerSummary, OwnedGame, List<PlayerSummary>, PlayerDetailsOverview>>();
                        for (int i = 0; i < ids.Count; i++)
                        {
                            Console.Clear();
                            Console.WriteLine("Loading Information. {0:00}%", (i * 100f)/ ids.Count);

                            var player = summaries.Players.Single(p => object.Equals(p.SteamId, ids[i]));

                            var psa = new PlayerServiceApi(steamKey);
                            var dota = psa.GetOwnedGames(ids[i], 570);
                            var game = dota.Games != null ? psa.GetOwnedGames(ids[i], 570).Games.FirstOrDefault(g => g.AppId == 570) : null;

                            var friends = player.CommunityVisibilityState == 3 ? api.GetFriendList(ids[i]).Friends : null;
                            var friendsInGame = friends != null ? friends.Select(f => summaries.Players.SingleOrDefault(p => object.Equals(p.SteamId, f.SteamId))).Where(f => f != null).ToList() : new List<PlayerSummary>();

                            PlayerDetailsOverview overview = null;
                            try
                            {
                                overview = DotaBuffScraper.RequestPlayerOverview(player.SteamId.ToEncodedIdString());
                            }
                            catch (Exception ex)
                            {
                                CConsole.WriteLine(ConsoleColor.DarkRed, ex.Message.ToString());
                            }

                            playerData.Add(Tuple.Create(player, game, friendsInGame, overview));
                        }
                        Console.Clear();
                        Console.WriteLine("Loaded Information. d2bl");

                        var parties = new List<HashSet<SteamId>>();
                        for (int i = 0; i < playerData.Count; i++)
                        {
                            var players = playerData[i].Item3
                                .Select(s => s.SteamId)
                                .Concat(playerData[i].Item1.SteamId)
                                .ToList();

                            var party = parties.Find(hs => players.Any(hs.Contains));
                            if (party == null)
                            {
                                party = new HashSet<SteamId>();
                                parties.Add(party);
                            }

                            for (int j = 0; j < players.Count; j++)
                                party.Add(players[j]);
                        }
                        parties = parties.Where(hs => hs.Count > 1).ToList();

                        for (int i = 0; i < playerData.Count; i++)
                        {
                            if (i == 0)
                                columns.AddLine("Radiant:");
                            else if (i == 5)
                                columns.AddLine("Dire:");

                            var player = playerData[i].Item1;
                            var game = playerData[i].Item2;
                            var overview = playerData[i].Item4;

                            var countryCode = "";
                            if (player.CommunityVisibilityState == 3)
                            {
                                countryCode = player.LocStateCode == null && player.LocCountryCode == null ?
                                    "not set" :
                                    string.Format("{0}, {1}", player.LocStateCode, player.LocCountryCode);
                            }

                            var party = parties.FindIndex(hs => hs.Contains(player.SteamId));

                            columns.AddLine(
                                i + 1,
                                player.PersonaName,
                                party != -1 ? (party + 1).ToString() : "",
                                player.TimeCreated != null ? player.TimeCreated.Value.ToString("MM-dd-yy") : "",
                                game != null ? string.Format("{0:0.##}", game.PlaytimeForever / 60f).ToString() : "",
                                countryCode,
                                overview != null ? (overview.Wins + overview.Losses).ToString() : "",
                                overview != null ? string.Format("{0:00.0}%", (overview.Wins * 100f) / (overview.Wins + overview.Losses)).ToString() : "",
                                overview != null ? overview.LastMatch.ToString("MM-dd-yy") : "");
                        }

                        var lines = columns.GetLines();
                        for (int i = 0; i < lines.Count; i++)
                        {
                            var color = i == 0 ? ConsoleColor.White : i < 7 ? ConsoleColor.Green : ConsoleColor.Red;
                            CConsole.WriteLine(color, lines[i]);
                        }
                    });
                };
                server.ReadLatestEntries();
            }

            Process.GetCurrentProcess().WaitForExit();
        }


        static void SteamIdsToDatabase()
        {
            var dbPath = Path.Combine(Environment.CurrentDirectory, "test.db");

            using (var conn = new SQLiteConnection("Data Source=" + dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = CreateTableQuery;
                    cmd.ExecuteNonQuery();
                }
            }

            var matches = new List<Match>();
            using (var server = new ServerLogMatchProvider(new FileInfo(@"E:\Steam\steamapps\common\dota 2 beta\dota\server_log.txt")))
            {
                server.MatchesFound += (sender, args) => matches.AddRange(args.Matches);
                server.ReadLatestEntries();
            }

            var context = new BanlistDbContext(new SQLiteConnection("Data Source=" + dbPath));
            context.Configuration.AutoDetectChangesEnabled = false;

            var steamIds = matches
                .SelectMany(m => m.SteamIds)
                .DistinctBy(s => s.ToEncodedId())
                .Select(s => new SteamIds { SteamIdEncoded = s.ToEncodedId(), SteamId_Y = s.Y, SteamId_Z = s.Z })
                .ToList();

            foreach (var sid in steamIds)
                context.SteamIds.Add(sid);

            for (int i = 0; i < matches.Count; i++)
            {
                var m = new Matches { LobbyId = matches[i].LobbyId };
                context.Matches.Add(m);

                for (int o = 0; o < matches[i].SteamIds.Count; o++)
                {
                    var enc = matches[i].SteamIds[o].ToEncodedId();
                    var id = steamIds.Single(s => s.SteamIdEncoded == enc);

                    context.MatchPlayers.Add(new MatchPlayers { Match = m, PlayerIndex = o, SteamId = id });
                }
            }

            context.SaveChanges();
        }

        const string CreateTableQuery = @"
    
    DROP TABLE if exists [Matches];
    
    DROP TABLE if exists [SteamIds];
    
    DROP TABLE if exists [MatchPlayers];

CREATE TABLE [Matches] (
    [Id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [LobbyId] integer   NOT NULL 
);
CREATE TABLE [SteamIds] (
    [Id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [SteamId_Y] integer   NOT NULL ,
    [SteamId_Z] integer   NOT NULL ,
    [SteamIdEncoded] integer UNIQUE  NOT NULL ,
    [CurrentName] nvarchar(2147483647)   NULL 
);
CREATE TABLE [MatchPlayers] (
    [Id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [PlayerIndex] integer   NOT NULL ,
    [Match_Id] integer   NOT NULL ,
    [SteamId_Id] integer   NOT NULL 
            
        ,CONSTRAINT [FK_MatchPlayersMatches]
            FOREIGN KEY ([Match_Id])
            REFERENCES [Matches] ([Id])					
            
                        
        ,CONSTRAINT [FK_MatchPlayersSteamIds]
            FOREIGN KEY ([SteamId_Id])
            REFERENCES [SteamIds] ([Id])					
            
            );";
    }
}
