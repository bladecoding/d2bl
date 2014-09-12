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
using Util;
using Newtonsoft.Json;

namespace Steam.JsonConverters
{
    public class IntegerToDateTime : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null && value.GetType() == typeof(DateTime))
            {
                writer.WriteValue(DateHelper.DateTimeToUnixTimestamp((DateTime)value));
                return;
            }
            throw new ArgumentException("Unable to convert type " + (value != null ? value.GetType().FullName : "null"), "objectType");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                long num;
                if (!long.TryParse((string)reader.Value, out num))
                    throw new ArgumentException("Unable to parse integer: " + reader.Value, "reader");
                return DateHelper.UnixTimeStampToDateTime(num);
            }
            if (NumericHelper.IsNumericType(reader.ValueType))
            {
                return DateHelper.UnixTimeStampToDateTime(Convert.ToInt64(reader.Value));
            }
            throw new ArgumentException("Unable to convert type " + reader.ValueType, "objectType");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || NumericHelper.IsNumericType(objectType);
        }
    }
}
