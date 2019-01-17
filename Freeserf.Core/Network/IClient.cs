﻿/*
 * IClient.cs - Interface for network clients
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
using System.Text;

namespace Freeserf.Network
{
    public interface IClient
    {
        uint PlayerIndex { get; }
        Game Game { get; set; }
        IServer Server { get; }

        void SendKeepAlive();
        void SendDisconnect();

        void SendPlayerStateUpdate(uint playerIndex);
        void SendMapStateUpdate();
        void SendGameStateUpdate();

        event EventHandler RequestReceived;

        void HandleRequest();
        void Respond();
    }

    public interface IClientFactory
    {
        IClient Create(uint playerIndex);
    }
}
