﻿/*
 * ConfigFile.cs - Configuration file read/write
 *
 * Copyright (C) 2017  Wicked_Digger <wicked_digger@mail.ru>
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Freeserf
{
    using Values = Dictionary<string, string>;
    using Sections = Dictionary<string, Dictionary<string, string>>;

    public class ConfigFile
    {
        Sections data = new Sections();

        public bool Load(string path)
        {
            if (!File.Exists(path))
            {
                Log.Error.Write("config", $"Failed to open config file '{path}'");
                return false;
            }

            using (var reader = new StreamReader(File.OpenRead(path), Encoding.ASCII))
            {
                return Read(reader);
            }
        }

        public bool Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var writer = new StreamWriter(File.Create(path), Encoding.ASCII))
                {
                    return Write(writer);
                }
            }
            catch
            {
                Log.Error.Write("config", $"Failed to open config file '{path}'");
                return false;
            }
        }

        public bool Read(StreamReader reader)
        {
            Values section = new Values();
            data["global"] = section;
            uint line_number = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var line = reader.ReadLine().Trim();
                ++line_number;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line[0] == '[')
                {
                    int end = line.IndexOf(']');

                    if (end == -1)
                    {
                        Log.Error.Write("config", $"Failed to parse config file ({line_number})");
                        return false;
                    }

                    string name = line.Substring(1, end - 1);

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Log.Error.Write("config", $"Failed to parse config file ({line_number})");
                        return false;
                    }

                    name = name.ToLower();

                    section = new Values();

                    data[name] = section;
                }
                else if (line[0] == ';' || line[0] == '#')
                {
                    // it's a comment line. drop it for now.
                }
                else
                {
                    if (line.Length != 0)
                    {
                        int pos = line.IndexOf('=');
                        string name = line.Substring(0, pos).Trim().ToLower();
                        string val = line.Substring(pos + 1).Trim().ToLower();
                        section[name] = val;
                    }
                }
            }

            return true;
        }

        public bool Write(StreamWriter writer)
        {
            foreach (var section in data)
            {
                writer.WriteLine("[" + section.Key + "]");

                foreach (var values in section.Value)
                {
                    writer.WriteLine("  " + values.Key + " = " + values.Value);
                }
            }

            return true;
        }

        public List<string> GetSections()
        {
            return data.Keys.ToList();
        }

        public List<string> GetValues(string section)
        {
            if (!data.ContainsKey(section))
                return new List<string>();

            return data[section].Keys.ToList();
        }

        public bool Contains(string section, string name)
        {
            if (!data.ContainsKey(section))
                return false;

            return data[section].ContainsKey(name);
        }

        public string Value(string section, string name, string defaultValue)
        {
            if (!Contains(section, name))
            {
                return defaultValue;
            }

            return data[section][name];
        }

        public T Value<T>(string section, string name, T defaultValue)
        {
            if (!Contains(section, name))
            {
                return defaultValue;
            }

            return data[section][name].GetValue<T>();
        }

        public void SetValue<T>(string section, string name, T value)
        {
            if (!data.ContainsKey(section))
            {
                var values = new Values();

                values[name] = value.ToString();

                data.Add(section, values);
            }
            else
            {
                data[section][name] = value.ToString();
            }
        }
    }
}