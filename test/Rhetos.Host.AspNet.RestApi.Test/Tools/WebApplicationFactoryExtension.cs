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

using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rhetos;
using Rhetos.Dom.DefaultConcepts;
using System;
using System.IO;

namespace Rhetos.Host.AspNet.RestApi.Test.Tools
{
    public static class WebApplicationFactoryExtensions
    {
        public static IWebHostBuilder MonitorLogging(this IWebHostBuilder builder, LogEntries logEntries, LogLevel minLogLevel = LogLevel.Information)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.Services.AddSingleton(logEntries);
                logging.Services.AddSingleton(new FakeLoggerOptions { MinLogLevel = minLogLevel });
            });

            return builder;
        }
    }
}
