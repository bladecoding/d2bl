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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Steam
{
    public class SteamId
    {
        private readonly SteamIdType m_Type;
        private readonly SteamIdUniverse m_X;
        private readonly long m_Y;
        private readonly long m_Z;

        public SteamId(SteamIdUniverse x, long y, long z)
        {
            m_Type = SteamIdType.Individual;
            m_X = x;
            m_Y = y;
            m_Z = z;
        }

        public static SteamId ParseEncodedId(string str)
        {
            var num = long.Parse(str);
            var y = num & 1;
            var z = num >> 1;
            return new SteamId(SteamIdUniverse.Public, y, z);
        }

        public static SteamId ParseSteamId(string str)
        {
            var match = Regex.Match(str, "STEAM_(?<x>\\d+):(?<y>\\d+):(?<z>\\d+)");
            if (!match.Success)
                throw new ArgumentException("SteamId is not in the correct format. Expecting 'STEAM_X:Y:Z'.", "str");

            var x = (SteamIdUniverse)int.Parse(match.Groups["x"].Value);
            var y = int.Parse(match.Groups["y"].Value);
            var z = long.Parse(match.Groups["z"].Value);
            return new SteamId(x, y, z);
        }

        public static SteamId ParseSteamId64(string str)
        {
            var num = long.Parse(str);
            num -= s_SteamId64Identifiers[SteamIdType.Individual];
            var y = num & 1;
            var z = num >> 1;
            return new SteamId(SteamIdUniverse.Public, y, z);
        }

        public long ToSteamId64()
        {
            long ident;
            if (!s_SteamId64Identifiers.TryGetValue(m_Type, out ident))
                throw new InvalidOperationException(string.Format("Unable to convert account type '{0}' to SteamId64.", m_Type));

            return (m_Z << 1) + ident + m_Y;
        }

        public string ToSteamIdString()
        {
            return string.Format("STEAM_{0}:{1}:{2}", (int)m_X, m_Y, m_Z);
        }

        public long ToEncodedId()
        {
            return ((m_Z << 1) | m_Y);
        }

        public string ToEncodedIdString()
        {
            return ToEncodedId().ToString();
        }

        public override string ToString()
        {
            return ToSteamIdString();
        }

        public SteamIdUniverse X
        {
            get { return m_X; }
        }

        public long Y
        {
            get { return m_Y; }
        }

        public long Z
        {
            get { return m_Z; }
        }

        static Dictionary<SteamIdType, long> s_SteamId64Identifiers = new Dictionary<SteamIdType, long>
        {
            {SteamIdType.Individual, 0x0110000100000000},
            {SteamIdType.Clan, 0x0170000000000000},
        };

        public override int GetHashCode()
        {
            return (int)(
                ((int)m_Type << 5) + (int)m_Type ^
                ((int)m_X << 5) + (int)m_X ^
                (m_Y << 5) + m_Y ^
                (m_Z << 5) + m_Z);
        }

        public override bool Equals(object obj)
        {
            var other = obj as SteamId;
            return other != null && other.m_X == m_X && other.m_Type == m_Type && other.m_Y == m_Y && other.m_Z == m_Z;
        }
    }

    public enum SteamIdUniverse
    {
        Unspecified = 0,
        Public = 1,
        Beta = 2,
        Internal = 3,
        Dev = 4,
        RC = 5,
    }

    public enum SteamIdType
    {
        Invalid = 0,
        Individual = 1,
        Multiseat = 2,
        GameServer = 3,
        AnonGameServer = 4,
        Pending = 5,
        ContentServer = 6,
        Clan = 7,
        Chat = 8,
        P2P = 9,
        AnonUser = 10,
    }
}
