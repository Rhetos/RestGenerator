/*
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

namespace Rhetos.JsonCommands.Host
{
    public class FakeLogger : ILogger
    {
        private readonly string categoryName;
        private readonly LogEntries logEntries;
        private readonly FakeLoggerOptions options;

        public FakeLogger(string categoryName, LogEntries logEntries, FakeLoggerOptions options)
        {
            this.categoryName = categoryName;
            this.logEntries = logEntries;
            this.options = options;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new FakeDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel >= options.MinLogLevel)
                logEntries.Add(logLevel, categoryName, formatter(state, exception));
        }
    }

    public class FakeLogger<T> : FakeLogger, ILogger<T>
    {
        public FakeLogger(LogEntries logEntries, FakeLoggerOptions options) : base(typeof(T).FullName, logEntries, options)
        {
        }
    }
}