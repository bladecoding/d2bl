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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Dota2BanlistCore;
using DotaBuff;
using Microsoft.Win32;
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
        static bool IsSteamKeyValid(string key)
        {
            return key.Length == 32 && key.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'));
        }

        static bool CheckVersion(out string msg)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var data = wc.DownloadString("http://d2blacklist.com/d2bl_update.txt");
                    var parts = data.Split('|');
                    msg = parts.Length > 1 ? parts[1] : "";
                    return parts[0] == "1.0";
                }
            }
            catch (Exception exception)
            {
                msg = "Exception checking version: " + exception.Message;
                return false;
            }
        }

        static readonly object LobbyLock = new object();
        [STAThread]
        static void Main()
        {
            var steamKey = Properties.Settings.Default.SteamKey;
            if (!IsSteamKeyValid(steamKey))
            {
                CConsole.WriteLine(ConsoleColor.Red, "Please provide a valid steam api key.");
                return;
            }

            var dotaPath = Properties.Settings.Default.Dota2Path;
            if (string.IsNullOrWhiteSpace(dotaPath))
            {
                var steamPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null) as string;
                if (steamPath == null)
                {
                    CConsole.WriteLine(ConsoleColor.Red, "Unable to automatically locate dota2. Please edit the settings and provide the path.");
                    return;
                }
                dotaPath = Path.Combine(steamPath, "steamapps\\common\\dota 2 beta\\game\\dota");
            }

            var serverLogPath = Path.Combine(dotaPath, "server_log.txt");
            if (!File.Exists(serverLogPath))
            {
                CConsole.WriteLine(ConsoleColor.Red, "Unable to locate dota2. Please make sure you have provided the correct path.");
                return;
            }

            CConsole.WriteLine(ConsoleColor.Yellow, "Waiting for lobby..");

            var useDotaBuff = Properties.Settings.Default.GrabStats;

            var server = new ServerLogMatchProvider(new FileInfo(serverLogPath));
            {
                server.MatchesFound += (sender, args) =>
                {
                    if (args.Matches.Count == 0)
                        return;

                    //Load the latest match if its from within 5 minutes ago. That way you can open d2bl a bit later and still get stats.
                    //if (args.Matches.Count > 1 && (DateTime.Now - args.Matches.Last().Date) > TimeSpan.FromMinutes(50000))
                       // return;

                    //Don't block the filewatcher!
                    //I feel dirty. Keep telling myself this is just for testing.
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        lock (LobbyLock)
                        {
                            var api = new SteamUserApi(steamKey);
                      
                            var ids = args.Matches.Last().SteamIds.ToArray();
                            var summaries = api.GetPlayerSummaries(ids);
                            var playerBans = api.GetPlayerBans(ids);

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
                                    "Abandons",
                                    "LastPlayed",
                                    "Bans");

                            var playerData = new List<Tuple<PlayerSummary, OwnedGame, List<PlayerSummary>, PlayerDetailsOverview>>();
                            for (int i = 0; i < ids.Length; i++)
                            {
                                Console.Clear();
                                Console.WriteLine("Loading Information. {0:00}%", (i * 100f) / ids.Length);

                                var player = summaries.Players.Single(p => object.Equals(p.SteamId, ids[i]));

                                var psa = new PlayerServiceApi(steamKey);
                                var dota = psa.GetOwnedGames(ids[i], 570);
                                var game = dota.Games != null ? dota.Games.FirstOrDefault(g => g.AppId == 570) : null;

                                var friends = player.IsPublic ? api.GetFriendList(ids[i]).Friends : null;
                                var friendsInGame = friends != null ? friends.Select(f => summaries.Players.SingleOrDefault(p => object.Equals(p.SteamId, f.SteamId))).Where(f => f != null).ToList() : new List<PlayerSummary>();

                                PlayerDetailsOverview overview = null;
                                try
                                {
                                    overview = useDotaBuff ? DotaBuffScraper.RequestPlayerOverview(player.SteamId.ToEncodedIdString()) : null;
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
                                else if (i == 10)
                                    columns.AddLine("Spectators:");

                                var player = playerData[i].Item1;
                                var game = playerData[i].Item2;
                                var overview = playerData[i].Item4;

                                var countryCode = "";
                                if (player.IsPublic)
                                {
                                    countryCode = player.LocStateCode == null && player.LocCountryCode == null ?
                                        "not set" :
                                        string.Format("{0}, {1}", player.LocStateCode, player.LocCountryCode);
                                }

                                var party = parties.FindIndex(hs => hs.Contains(player.SteamId));
                                var banInfo = playerBans.Players.FirstOrDefault(p => Equals(p.SteamId, player.SteamId));

                                columns.AddLine(
                                    i + 1,
                                    player.PersonaName,
                                    party != -1 ? (party + 1).ToString() : "",
                                    player.TimeCreated != null ? player.TimeCreated.Value.ToString("MM-dd-yy") : "",
                                    game != null ? string.Format("{0:0.##}", game.PlaytimeForever / 60f).ToString() : "",
                                    countryCode,
                                    overview != null ? (overview.Wins + overview.Losses).ToString() : "",
                                    overview != null ? string.Format("{0:00.0}%", (overview.Wins * 100f) / (overview.Wins + overview.Losses + overview.Abandons)).ToString() : "",
                                    overview != null ? overview.Abandons.ToString() : "",
                                    overview != null ? overview.LastMatch.ToString("MM-dd-yy") : "",
                                    banInfo != null ? string.Format("{0}_{1}", banInfo.NumberOfVACBans, banInfo.DaysSinceLastBan) : "?");
                            }

                            var lines = columns.GetLines();
                            for (int i = 0; i < lines.Count; i++)
                            {
                                var color = i == 0 ? ConsoleColor.White : i < 7 ? ConsoleColor.Green : ConsoleColor.Red;
                                CConsole.WriteLine(color, lines[i]);
                            }
                        }
                    });
                };
                server.ReadLatestEntries();
            }

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
