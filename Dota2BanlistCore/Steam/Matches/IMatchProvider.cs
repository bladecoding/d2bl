using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dota2BanlistCore
{
    public interface IMatchProvider
    {
         event EventHandler<MatchesFoundEventArgs> MatchesFound;
    }

    public class MatchesFoundEventArgs : EventArgs
    {
        readonly IList<Match> m_Matches;

        public MatchesFoundEventArgs(IList<Match> matches)
        {
            m_Matches = matches;
        }

        public IList<Match> Matches
        {
            get { return m_Matches; }
        }
    }
}
