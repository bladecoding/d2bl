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

using System.Collections.Generic;

namespace DotaBuff
{
    public class HeroDetailsOverview
    {
        public string HeroName { get; set; }
        public IList<VersusDetails> BestVersus { get; set; }
        public IList<VersusDetails> WorstVersus { get; set; }
    }
}
