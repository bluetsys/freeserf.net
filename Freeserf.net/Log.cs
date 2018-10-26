﻿/*
 * Log.cs - Logging
 *
 * Copyright (C) 2012-2016  Jon Lund Steffensen <jonlst@gmail.com>
 * Copyright (C) 2018       Robert Schneckenhaus <robert.schneckenhaus@web.de>
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

/* Log levels determine the importance of a log message. Use the
   following guidelines when deciding which level to use.

   - ERROR: The _user_ must take action to continue usage.
   Must usually be followed by exit(..) or abort().
   - WARN: Warn the _user_ of an important problem that the
   user might be able to resolve.
   - INFO: Information that is of general interest to _users_.
   - DEBUG: Log a problem that a _developer_ should look
   into and fix.
   - VERBOSE: Log any other information that a _developer_ might
   be interested in. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Freeserf
{
    public class Log
    {
        public enum Level
        {
            Verbose = 0,
            Debug,
            Info,
            Warn,
            Error,

            Max
        }

        public class Stream : IDisposable
        {
            protected StreamWriter streamWriter;

            public System.IO.Stream UnderlyingStream => streamWriter?.BaseStream ?? streamWriter.BaseStream;

            public Stream(System.IO.Stream stream)
            {
                streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
            }

            public static Stream operator +(Stream stream, string val)
            {
                stream.streamWriter.Write(val);

                return stream;
            }

            public static Stream operator +(Stream stream, int val)
            {
                stream.streamWriter.Write(val);

                return stream;
            }


            #region IDisposable Support

            private bool disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        if (streamWriter != null)
                        {
                            streamWriter.WriteLine();
                            streamWriter.Flush();
                        }
                    }

                    disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion

        }

        public class Logger
        {
            protected Level level;
            protected string prefix;
            protected System.IO.Stream stream;
            protected static System.IO.Stream dummy = null;

            public Logger(Level level, string prefix)
            {
                this.level = level;
                this.prefix = prefix;

                ApplyLevel();
            }

            public virtual void Write(string subsystem, string text)
            {
                var stream = this[subsystem];

                stream += text;
            }

            public virtual Stream this[string subsystem]
            {
                get
                {
                    if (this.stream == null)
                        return null;

                    var stream = new Stream(this.stream);

                    stream += prefix + ": [" + subsystem + "] ";

                    return stream;
                }
            }

            public void ApplyLevel()
            {
                if (level < Log.level)
                {
                    stream = dummy;
                }
                else
                {
                    stream = Log.stream;
                }
            }
        }

        public static void SetFile(System.IO.Stream stream)
        {
            Log.stream = stream;
        }

        public static void SetLevel(Level level)
        {
            Log.level = level;

            Verbose.ApplyLevel();
            Debug.ApplyLevel();
            Info.ApplyLevel();
            Warn.ApplyLevel();
            Error.ApplyLevel();
        }

        public static Logger Verbose = new Logger(Level.Verbose, "Verbose");
        public static Logger Debug = new Logger(Level.Debug, "Debug");
        public static Logger Info = new Logger(Level.Info, "Info");
        public static Logger Warn = new Logger(Level.Warn, "Warning");
        public static Logger Error = new Logger(Level.Error, "Error");

        protected static System.IO.Stream stream = Console.OpenStandardOutput();

#if DEBUG
        protected static Level level = Level.Debug;
#else
        protected static Level level = Level.LevelInfo;
#endif

    }
}
