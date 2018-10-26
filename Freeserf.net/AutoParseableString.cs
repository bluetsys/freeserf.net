﻿/*
 * AutoParseableString.cs - Kind of a replacement for std::stringstream.
 *
 * Copyright (C) 2018  Robert Schneckenhaus <robert.schneckenhaus@web.de>
 *
 * This file is part of freeserf.net. freeserf.net is based on freeserf.
 *
 * freeserf.net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * freeserf.net is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with freeserf.net. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Text.RegularExpressions;

namespace Freeserf
{
    /// <summary>
    ///  Replacement for std::stringstream in CommandLine
    /// </summary>
    public class AutoParseableString
    {
        string content = "";
        static readonly Regex NoDigitRegex = new Regex("[^0-9]", RegexOptions.Compiled);

        public AutoParseableString(string content)
        {
            this.content = content;
        }

        public void Skip(int num)
        {
            num = Math.Min(num, content.Length);

            content = content.Substring(num);
        }

        public void Retrieve(out uint val)
        {
            Retrieve(out int temp);

            val = (uint)temp;
        }


        public void Retrieve(out int val)
        {
            var match = NoDigitRegex.Match(content);

            if (!match.Success || match.Index == 0)
            {
                val = -1;
                return;
            }

            string valueString = content.Substring(0, match.Index);

            content = content.Substring(match.Index);

            if (!int.TryParse(valueString, out val))
                val = -1;
        }

        public void Retrieve(out string val)
        {
            int end = content.IndexOfAny(new char[] { ' ', '\t' });

            if (end == -1)
            {
                val = content;
                content = "";
            }
            else
            {
                val = content.Substring(0, end);
                content = content.Substring(end + 1);
            }
        }

        public string ReadLine()
        {
            var temp = content;

            content = "";

            return temp;
        }
    }
}