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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rhetos;
using Rhetos.JsonCommands.Host.Filters;
using Rhetos.JsonCommands.Host.Utilities;
using System;

namespace Rhetos.JsonCommands.Host
{
    public static class RhetosJsonCommandsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the required services for Rhetos JsonCommands API.
        /// </summary>
        public static RhetosServiceCollectionBuilder AddJsonCommands(this RhetosServiceCollectionBuilder builder,
            Action<JsonCommandsOptions> configureOptions = null)
        {
            builder.AddJsonCommandsFilters();

            builder.Services.AddOptions();

            if (configureOptions != null)
            {
                builder.Services.Configure<JsonCommandsOptions>(configureOptions);
            }

            return builder;
        }

        /// <summary>
        /// Adds Rhetos specific filters into the ASP.NET request processing pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static RhetosServiceCollectionBuilder AddJsonCommandsFilters(this RhetosServiceCollectionBuilder builder)
        {
            builder.Services.TryAddScoped<ErrorReporting>();
            builder.Services.TryAddScoped<ApiExceptionFilter>();
            builder.Services.TryAddScoped<ApiCommitOnSuccessFilter>();

            return builder;
        }
    }
}
