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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ninject;
using Steam;

namespace Dota2BanlistCore
{
    public class ServerLogMatchProvider : IDisposable, IMatchProvider
    {

        const string CrazyRegex = @"(?<date>\d+/\d+/\d+)\s+-\s+(?<time>\d+:\d+:\d+):\s+(?<ipaddr>\d+\.\d+\.\d+\.\d+:\d+)\s+\(\w+\s+(?<lobbyid>\d+)\s+(?<gamemode>\w+)\s+((?<ids>\d:\[\w+:\d+:\d+\])\s*)+\)(\s*\(\w+\s+\d+\s+((?<partyids>\d:\[\w+:\d+:\d+\])\s*)+\))?";
        const string SubCrazyRegex = @"\d+:\[\w+:\d+:(?<id>\d+)\]";

        readonly FileInfo m_ServerLogFile;
        FileSystemWatcher m_Watcher;
        StreamReader m_ServerReader;

        public ServerLogMatchProvider(FileInfo serverLogFile)
        {
            m_ServerLogFile = serverLogFile;
            m_ServerReader = new StreamReader(m_ServerLogFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            m_Watcher = new FileSystemWatcher(serverLogFile.DirectoryName, serverLogFile.Name);
            m_Watcher.EnableRaisingEvents = true;
            m_Watcher.Changed += watcher_Changed;
        }

        public void ReadLatestEntries()
        {
            ReadEntries();
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (m_Watcher == null)
                return;

            ReadEntries();
        }

        readonly object m_FileLock = new object();
        void ReadEntries()
        {
            //Inb4 regex is a bottleneck :|
            var matches = new List<Match>();

            lock (m_FileLock)
            {
                string line;
                while ((line = m_ServerReader.ReadLine()) != null)
                {
                    var match = Regex.Match(line, CrazyRegex);
                    if (match.Success)
                    {
                        long lobbyId;
                        if (!long.TryParse(match.Groups["lobbyid"].Value, out lobbyId))
                            continue;

                        DateTime dt;
                        if (!DateTime.TryParseExact(match.Groups["date"].Value + match.Groups["time"].Value, "MM/dd/yyyyHH:mm:ss", null, DateTimeStyles.None, out dt))
                            continue;

                        IPEndPoint ip;
                        if (!TryCreateIpEndPoint(match.Groups["ipaddr"].Value, out ip))
                            continue;

                        string gamemode = match.Groups["gamemode"].Value;

                        IList<SteamId> steamIds;
                        var idsGroup = match.Groups["ids"];
                        if (!TryParseSteamIds(idsGroup.Captures.Cast<Capture>().Select(c => c.Value), out steamIds))
                            continue;

                        IList<SteamId> partySteamIds;
                        var partyIdsGroup = match.Groups["partyids"];
                        if (partyIdsGroup == null || !TryParseSteamIds(partyIdsGroup.Captures.Cast<Capture>().Select(c => c.Value), out partySteamIds))
                            partySteamIds = new List<SteamId>();

                        matches.Add(new Match { LobbyId = lobbyId, Date = dt, IpAddress = ip, Gamemode = gamemode, SteamIds = steamIds, PartySteamIds = partySteamIds });
                    }
                }
            }

            if (matches.Count > 0 && MatchesFound != null)
                MatchesFound(this, new MatchesFoundEventArgs(matches));

        }

        static bool TryParseSteamIds(IEnumerable<string> strs, out IList<SteamId> ids)
        {
            ids = new List<SteamId>();
            foreach (var str in strs)
            {
                var match = Regex.Match(str, SubCrazyRegex);
                if (!match.Success)
                    return false;

                ids.Add(SteamId.ParseEncodedId(match.Groups["id"].Value));
            }
            return ids.Count > 0;
        }

        static bool TryCreateIpEndPoint(string endPoint, out IPEndPoint ipEndPoint)
        {
            ipEndPoint = null;

            var ep = endPoint.Split(':');
            if (ep.Length != 2)
                return false;

            IPAddress ip;
            int port;

            if (!IPAddress.TryParse(ep[0], out ip))
                return false;

            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
                return false;

            ipEndPoint = new IPEndPoint(ip, port);
            return true;
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_Watcher != null)
                {
                    m_Watcher.Dispose();
                    m_Watcher = null;
                }
                if (m_ServerReader != null)
                {
                    m_ServerReader.Dispose();
                    m_ServerReader = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ServerLogMatchProvider()
        {
            Dispose(false);
        }

        #endregion

        public event EventHandler<MatchesFoundEventArgs> MatchesFound;
    }
}
