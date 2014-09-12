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
using System.Web;

namespace Steam.Api
{
    public abstract class BaseServiceApi
    {
        const string c_BaseUrl = "http://api.steampowered.com";
        const string c_ApiFormat = "json";


        protected string m_Key;
        protected BaseServiceApi(string key)
        {
            m_Key = key;
        }

        protected abstract string ServiceName { get; }

        protected string GetCallUrl(string func, string version, params Tuple<string, string>[] param)
        {
            var urlParams = new[] 
            { 
                Tuple.Create("key", m_Key), 
                Tuple.Create("format", c_ApiFormat)
            }.Concat(param);

            var urlParamsStr = string.Join("&", urlParams.Where(p => p != null).Select(kv => HttpUtility.UrlEncode(kv.Item1) + "=" + HttpUtility.UrlEncode(kv.Item2)));
            return string.Format("{0}/{1}/{2}/{3}?{4}", c_BaseUrl, ServiceName, func, version, urlParamsStr);
        }
    }
}
