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

namespace Dota2BanlistCore
{
    public class ServerLogMatchProvider : IDisposable, IMatchProvider
    {
        const string CrazyRegex = @"(?<date>\d+/\d+/\d+)\s+-\s+(?<time>\d+:\d+:\d+):\s+(?<ipaddr>\d+\.\d+\.\d+\.\d+:\d+)\s+\(\w+\s+\d+\s+(?<gamemode>\w+)\s+((?<ids>\d:\[\w+:\d+:\d+\])\s*)+\)";
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

            ThreadPool.QueueUserWorkItem(ReadLatestEntries);
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (m_Watcher == null)
                return;

            ReadLatestEntries("grrr");
        }

        void ReadLatestEntries(object state)
        {
            //Inb4 regex is a bottleneck :|

            var matches = new List<Match>();
            
            string line;
            while ((line = m_ServerReader.ReadLine()) != null)
            {
                var match = Regex.Match(line, CrazyRegex);
                if (match.Success)
                {
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

                    matches.Add(new Match { Date = dt, IpAddress = ip, Gamemode = gamemode, SteamIds = steamIds});
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
