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

namespace Dota2BanlistCore
{
    /// <summary>
    /// Console methods with color parameters.
    /// </summary>
    public static class CConsole
    {
        public static void Write(ConsoleColor fore, String format, params Object[] args)
        {
            Write(fore, string.Format(format, args));
        }
        public static void Write(ConsoleColor fore, String input)
        {
            var f = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = fore;

                Console.Write(input);
            }
            finally
            {
                Console.ForegroundColor = f;
            }
        }
        public static void Write(ConsoleColor fore, ConsoleColor back, String format, params Object[] args)
        {
            Write(fore, back, string.Format(format, args));
        }
        public static void Write(ConsoleColor fore, ConsoleColor back, String input)
        {
            var f = Console.ForegroundColor;
            var b = Console.BackgroundColor;
            try
            {
                Console.ForegroundColor = fore;
                Console.BackgroundColor = back;

                Console.Write(input);
            }
            finally
            {
                Console.ForegroundColor = f;
                Console.BackgroundColor = b;
            }
        }
        public static void WriteLine(ConsoleColor fore, String format, params Object[] args)
        {
            WriteLine(fore, string.Format(format, args));
        }
        public static void WriteLine(ConsoleColor fore, String input)
        {
            var f = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = fore;

                Console.WriteLine(input);
            }
            finally
            {
                Console.ForegroundColor = f;
            }
        }
        public static void WriteLine(ConsoleColor fore, ConsoleColor back, String format, params Object[] args)
        {
            WriteLine(fore, back, string.Format(format, args));
        }
        public static void WriteLine(ConsoleColor fore, ConsoleColor back, String input)
        {
            var f = Console.ForegroundColor;
            var b = Console.BackgroundColor;
            try
            {
                Console.ForegroundColor = fore;
                Console.BackgroundColor = back;

                Console.WriteLine(input);
            }
            finally
            {
                Console.ForegroundColor = f;
                Console.BackgroundColor = b;
            }
        }
    }
}
