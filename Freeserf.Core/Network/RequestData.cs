﻿/*
 * RequestData.cs - Data for network requests
 *
 * Copyright (C) 2019  Robert Schneckenhaus <robert.schneckenhaus@web.de>
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freeserf.Network
{
    public enum Request : byte
    {
        None = 0,
        Heartbeat,
        StartGame,
        Disconnect,
        LobbyData,
        PlayerData,
        MapData,
        GameData
    }

    public partial class Global
    {
        public const byte SpontaneousMessage = 0xff;

        static byte CurrentMessageIndex = 0;
        static object MessageIndexLock = new object();

        public static byte GetNextMessageIndex()
        {
            lock (MessageIndexLock)
            {
                byte currentMessageIndex = CurrentMessageIndex;

                if (CurrentMessageIndex == SpontaneousMessage - 1)
                    CurrentMessageIndex = 0;
                else
                    ++CurrentMessageIndex;

                return currentMessageIndex;
            }
        }
    }

    public class RequestData : INetworkData
    {
        const int Size = 4;

        public NetworkDataType Type => NetworkDataType.Request;

        public byte Number
        {
            get;
            private set;
        } = 0;

        public Request Request
        {
            get;
            private set;
        } = Request.None;

        public RequestData()
        {
            // use when parsing the data
        }

        public RequestData(byte number, Request request)
        {
            Number = number;
            Request = request;
        }

        public int GetSize()
        {
            return Size;
        }

        public INetworkData Parse(byte[] rawData)
        {
            if (rawData.Length != Size)
                throw new ExceptionFreeserf($"Request length must be {Size}.");

            Number = rawData[2];

            var possibleValues = Enum.GetValues(typeof(Request));

            foreach (Request possibleValue in possibleValues)
            {
                if ((byte)possibleValue == rawData[3])
                {
                    Request = possibleValue;
                    return this;
                }
            }

            throw new ExceptionFreeserf("Invalid request.");
        }

        public void Send(IRemote destination)
        {
            List<byte> rawData = new List<byte>(GetSize());

            rawData.AddRange(BitConverter.GetBytes((UInt16)Type));
            rawData.Add(Number);
            rawData.Add((byte)Request);

            destination?.Send(rawData.ToArray());
        }
    }
}
