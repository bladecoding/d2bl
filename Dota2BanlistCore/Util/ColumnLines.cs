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

namespace Util
{
    public class ColumnLines
    {
        private List<int> m_MaxLengths = new List<int>();
        private List<string[]> m_Lines = new List<string[]>();

        public int Spacing { get; set; }
        public char SpacingChar { get; set; }

        public ColumnLines()
        {
            Spacing = 4;
            SpacingChar = ' ';
        }
        public void AddLine(params object[] line)
        {
            AddLine(line.Select(l => l.ToString()).ToArray());
        }
        public void AddLine(params string[] line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (i < m_MaxLengths.Count)
                    m_MaxLengths[i] = line[i].Length > m_MaxLengths[i] ? line[i].Length : m_MaxLengths[i];
                else
                    m_MaxLengths.Add(line[i].Length);
            }
            m_Lines.Add(line);
        }

        public List<string> GetLines()
        {
            return m_Lines.Select(cols => string.Concat(cols.Select((c, i) => c.PadRight(m_MaxLengths[i] + Spacing)))).ToList();
        }
    }
}
