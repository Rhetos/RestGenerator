﻿/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Rhetos.JsonCommands.Host
{
    public class LogEntries : ConcurrentQueue<FakeLogEntry>
    {
        public void Add(LogLevel logLevel, string categoryName, string message)
        {
            Enqueue(new FakeLogEntry(logLevel, categoryName, message));
        }
    }
}