using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dota2BanlistCore
{
    public class SteamId
    {
        private long m_Y;
        private long m_Z;

        public SteamId(long y, long z)
        {
            m_Y = y;
            m_Z = z;
        }

        public static SteamId ParseEncodedId(string str)
        {
            var num = long.Parse(str);
            var y = num & 1;
            var z = num >> 1;
            return new SteamId(y, z);
        }

        public static SteamId ParseSteamId(string str)
        {
            var match = Regex.Match(str, "STEAM_(?<x>\\d+):(?<y>\\d+):(?<z>\\d+)");
            if (!match.Success)
                throw new ArgumentException("SteamId is not in the correct format. Expecting 'STEAM_X:Y:Z'.", "str");

            var y = int.Parse(match.Groups["y"].Value);
            var z = long.Parse(match.Groups["z"].Value);
            return new SteamId(y, z);
        }

        public string ToSteamIdString()
        {
            return "STEAM_1:" + m_Y + ":" + m_Z;
        }

        public string ToEncodedIdString()
        {
            return ((m_Z << 1) | m_Y).ToString();
        }

        public override string ToString()
        {
            return ToSteamIdString();
        }
    }
}
