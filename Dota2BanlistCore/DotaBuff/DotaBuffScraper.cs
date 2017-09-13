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
using System.Web;
using HtmlAgilityPack;

namespace DotaBuff
{
    public static class DotaBuffScraper
    {
        /// <summary>
        /// Expects the Steam encoded id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PlayerDetailsOverview RequestPlayerOverview(string id)
        {
            //dotabuff doesn't like WebClient :(

            //Most plyaed is on the main page. (http://dotabuff.com/players/<id>)
            //Real matches is on a separate page. (http://dotabuff.com/players/<id>/matches)

            try
            {
                var details = new PlayerDetailsOverview();

                var doc = RequestDocument(string.Format("https://www.dotabuff.com/players/{0}", HttpUtility.UrlEncode(id)));

                var avatarNode = GetNodeFromXpathOrError(doc.DocumentNode, "//*[@class=\"header-content-avatar\"]");
                var nameNode = GetNodeFromXpathOrError(avatarNode, ".//img[contains(@class,'image-player')]");

                var secondaryNode = doc.DocumentNode.SelectSingleNode("//*[@class=\"header-content-secondary\"]");
                if (secondaryNode == null)
                    return null; //Private profile :(

                var lastMatchNode = GetNodeFromXpathOrError(secondaryNode, ".//time");
                var wonNode = GetNodeFromXpathOrError(secondaryNode, ".//span[@class='wins']");
                var lostNode = GetNodeFromXpathOrError(secondaryNode, ".//span[@class='losses']");
                var abandonNode = secondaryNode.SelectSingleNode(".//span[@class='abandons']");


                details.PlayerName = HtmlEntity.DeEntitize(nameNode.Attributes["alt"].Value);
                details.LastMatch = ParseNodeDateTime(lastMatchNode);
                details.Wins = int.Parse(wonNode.InnerText, NumberStyles.Any);
                details.Losses = int.Parse(lostNode.InnerText, NumberStyles.Any);
                details.Abandons = abandonNode != null ? int.Parse(abandonNode.InnerText, NumberStyles.Any) : 0;
                //details.MostPlayedHeroes = GetMostPlayedHeroes(doc.DocumentNode);
                //details.LatestRealMatches = RequestLatestMatches(id);


                return details;
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        public static HeroDetailsOverview RequestHeroOverview(string heroName)
        {
            try
            {
                var doc = RequestDocument(string.Format("http://dotabuff.com/heroes/{0}/_versus", HttpUtility.UrlEncode(heroName)));
                return GetHeroOverview(doc, heroName);
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }
        private static HeroDetailsOverview GetHeroOverview(HtmlDocument doc, string heroName)
        {
            var details = new HeroDetailsOverview();

            var headers = doc.DocumentNode.SelectNodes(".//section/header");
            var best = headers.Single(n => n.InnerText.Contains("Best")).NextSibling;
            var worst = headers.Single(n => n.InnerText.Contains("Worst")).NextSibling;

            details.HeroName = heroName;
            details.BestVersus = GetVersusDetails(best);
            details.WorstVersus = GetVersusDetails(worst);

            return details;
        }
        private static IList<VersusDetails> GetVersusDetails(HtmlNode doc)
        {
            var heroes = new List<VersusDetails>();
            var table = GetNodeFromXpathOrError(doc, ".//tbody");
            var rows = table.SelectNodes(".//tr");
            foreach (var row in rows)
            {
                var details = new VersusDetails();

                var columnIdx = 1; //Skip the icon column
                var columns = row.SelectNodes(".//td");

                var heroNode = GetHeroNameFromOverviewLink(columns[columnIdx++]);
                var advNode = columns[columnIdx++];
                var winRateNode = columns[columnIdx++];
                var matchesNode = columns[columnIdx++];

                details.HeroName = heroNode;
                details.Advantage = double.Parse(advNode.InnerText.TrimEnd('%'), NumberStyles.Any);
                details.WinRate = double.Parse(winRateNode.InnerText.TrimEnd('%'), NumberStyles.Any);
                details.Matches = long.Parse(matchesNode.InnerText.TrimEnd('%'), NumberStyles.Any);

                heroes.Add(details);
            }
            return heroes;
        }
        private static IList<MostPlayedHero> GetMostPlayedHeroes(HtmlNode doc)
        {
            var heroes = new List<MostPlayedHero>();
            var table = GetNodeFromXpathOrError(doc, ".//div[@class='heroes-overview']/section/article/table/tbody");
            var rows = table.SelectNodes(".//div[@class='r-row']");
            foreach (var row in rows)
            {
                var hero = new MostPlayedHero();

                var columnIdx = 1; //Skip the icon column
                var columns = row.SelectNodes(".//div[@class='r-body']");

                var heroNode = GetHeroNameFromOverviewLink(columns[columnIdx++]);
                var matchesNode = columns[columnIdx++];
                var winRateNode = columns[columnIdx++];
                var kdaRatioNode = columns[columnIdx++];

                hero.HeroName = heroNode;
                hero.MatchesPlayed = int.Parse(matchesNode.InnerText, NumberStyles.Any);
                hero.WinRate = double.Parse(winRateNode.InnerText.TrimEnd('%'), NumberStyles.Any);
                hero.KdaRatio = double.Parse(kdaRatioNode.InnerText.TrimEnd('%'), NumberStyles.Any);

                heroes.Add(hero);
            }

            return heroes;
        }


        private static IList<LatestRealMatch> RequestLatestMatches(string id)
        {
            try
            {
                var doc = RequestDocument(string.Format("http://dotabuff.com/players/{0}/matches", HttpUtility.UrlEncode(id)));
                return GetLatestMatches(doc);
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        private static IList<LatestRealMatch> GetLatestMatches(HtmlDocument doc)
        {
            var matches = new List<LatestRealMatch>();
            var rows = doc.DocumentNode.SelectNodes(".//tbody/tr");
            foreach (var row in rows)
            {
                var match = new LatestRealMatch();

                var columnIdx = 1; //Skip the icon column
                var columns = row.SelectNodes(".//td");

                var heroNode = GetHeroNameFromMatchesLink(columns[columnIdx++]);
                var wonNode = columns[columnIdx].SelectSingleNode(".//a[@class='won']");
                var matchNode = GetNodeFromXpathOrError(columns[columnIdx], ".//a");
                var timeNode = GetNodeFromXpathOrError(columns[columnIdx++], ".//time");
                var gameModeNode = GetNodeFromXpathOrError(columns[columnIdx++], ".//div[@class='subtext']");
                var durationNode = columns[columnIdx++];
                var killsNode = GetNodeFromXpathOrError(columns[columnIdx++], ".//span[@class='kda-record']");
                var killsDeathsAssists = ParseKillsDeathsAssists(HtmlEntity.DeEntitize(killsNode.InnerText));

                var matchId = Regex.Match(matchNode.Attributes["href"].Value, "(\\d+)$", RegexOptions.RightToLeft);
                if (!matchId.Success)
                    throw new InvalidOperationException("match id not found");

                match.HeroName = heroNode;
                match.Won = wonNode != null;
                match.Date = ParseNodeDateTime(timeNode);
                match.MatchId = long.Parse(matchId.Groups[1].Value, NumberStyles.Any);
                match.GameMode = gameModeNode.InnerText;
                match.Duration = TimeSpan.ParseExact(durationNode.InnerText, new[] { "mm':'ss", "hh':'mm':'ss" }, null);
                match.Kills = killsDeathsAssists.Item1;
                match.Deaths = killsDeathsAssists.Item2;
                match.Assists = killsDeathsAssists.Item3;

                matches.Add(match);
            }

            return matches;
        }

        private static string GetHeroNameFromOverviewLink(HtmlNode node)
        {
            var linkNode = node
                .SelectNodes(".//a")
                .FirstOrDefault(n => n.Attributes["href"] != null && n.Attributes["href"].Value.Contains("hero"));
            if (linkNode == null)
                throw new InvalidOperationException("hero link not found");
            return linkNode.InnerText;
        }
        private static string GetHeroNameFromMatchesLink(HtmlNode node)
        {
            var linkNode = node
                .SelectNodes(".//a")
                .FirstOrDefault(n => n.Attributes["href"] != null && n.Attributes["href"].Value.StartsWith("/matches/"));
            if (linkNode == null)
                throw new InvalidOperationException("hero link not found");
            return linkNode.InnerText;
        }

        /// <summary>
        /// Format is "1 / 2 / 3"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static Tuple<int, int, int> ParseKillsDeathsAssists(string input)
        {
            var match = Regex.Match(input, @"(?<kills>\d+)\s*\/\s*(?<deaths>\d+)\s*\/\s*(?<assists>\d+)");
            if (!match.Success)
                throw new InvalidOperationException("KillsDeathsAssists not valid: " + input);

            return Tuple.Create(
                int.Parse(match.Groups["kills"].Value),
                int.Parse(match.Groups["deaths"].Value),
                int.Parse(match.Groups["assists"].Value));
        }

        private static DateTime ParseNodeDateTime(HtmlNode node)
        {
            DateTime dt;
            if (!node.Attributes.Contains("datetime") ||
                !DateTime.TryParseExact(node.Attributes["datetime"].Value, "yyyy-MM-dd'T'HH:mm:ssK", null, DateTimeStyles.None, out dt))
                throw new InvalidOperationException("Datetime not valid: " + node.Attributes["datetime"].Value);
            return dt;
        }

        private static HtmlNode GetNodeFromXpathOrError(HtmlNode node, string xpath)
        {
            var selected = node.SelectSingleNode(xpath);
            if (selected == null)
                throw new InvalidOperationException(xpath + " not found");
            return selected;
        }

        private static HtmlDocument RequestDocument(string url)
        {
            var html = RequestHtml(url);
            var doc = new HtmlDocument();
            doc.Load(new StringReader(html));

            if (doc.ParseErrors != null && doc.ParseErrors.Any(p => p.Code != HtmlParseErrorCode.EndTagNotRequired))
                throw new InvalidOperationException("Parse error: " + doc.ParseErrors.First().Reason);

            if (doc.DocumentNode == null)
                throw new InvalidOperationException("Doc root is null.");

            return doc;
        }

        private static string RequestHtml(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36";
            req.ServicePoint.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            using (var resp = req.GetResponse())
            {
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

    //Models for the site. They match with the text on the page.
    //Currently this is missing Lifetime stats, team membership, friends and aliases
}
