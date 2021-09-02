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

using Microsoft.Extensions.DependencyInjection.Extensions;
using Rhetos;
using Rhetos.Host.AspNet.RestApi;
using Rhetos.Host.AspNet.RestApi.Filters;
using Rhetos.Host.AspNet.RestApi.Metadata;
using Rhetos.Host.AspNet.RestApi.Utilities;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RhetosRestApiServiceCollectionExtensions
    {
        public static RhetosServiceCollectionBuilder AddRestApi(this RhetosServiceCollectionBuilder builder,
            Action<RestApiOptions> configureOptions = null)
        {
            builder.AddRestApiFilters();

            builder.Services.AddOptions();

            if (configureOptions != null)
            {
                builder.Services.Configure<RestApiOptions>(configureOptions);
            }

            builder.Services.TryAddScoped<QueryParameters>();
            builder.Services.TryAddScoped<ServiceUtility>();
            builder.Services.TryAddSingleton<ControllerRestInfoRepository>();
            
            return builder;
        }

        /// <summary>
        /// Adds Rhetos specific filters into the ASP.NET request processing pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static RhetosServiceCollectionBuilder AddRestApiFilters(this RhetosServiceCollectionBuilder builder)
        {
            builder.Services.TryAddScoped<JsonErrorHandler>();
            builder.Services.TryAddScoped<ApiExceptionFilter>();
            builder.Services.TryAddScoped<ApiCommitOnSuccessFilter>();

            return builder;
        }
    }
}
