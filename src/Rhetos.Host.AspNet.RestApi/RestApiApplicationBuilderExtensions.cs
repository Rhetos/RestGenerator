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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rhetos;
using Rhetos.Host.AspNet.RestApi;
using Rhetos.Host.AspNet.RestApi.Metadata;

namespace Rhetos
{
    public static class RestApiApplicationBuilderExtensions
    {
        /// <summary>
        /// Add discovered Rhetos controllers to MVC feature pipeline and maps them to their respective routes.
        /// Call before 'app.UseEndpoints()', `app.MapControllers()` and `app.UseSwagger()`.
        /// </summary>
        public static IApplicationBuilder UseRhetosRestApi(this IApplicationBuilder app)
        {
            var controllerRestInfoRepository = app.ApplicationServices.GetService<ControllerRestInfoRepository>();

            if (controllerRestInfoRepository == null)
            {
                throw new InvalidOperationException(
                    $"Can't resolve {nameof(ControllerRestInfoRepository)} from dependency injection. Did you forget to 'AddRhetos().AddRestApi()' in ConfigureServices of Startup.cs file?");
            }

            // Slightly hacky way to modify features and conventions AFTER service provider has already been built
            // Due to this, it is important that this method is executed prior to any others that force feature enumeration
            // Feature enumeration will usually happen during any endpoint mapping, so this should occur prior to app.UseEndpoints or any middleware that invokes it
            // Also, due to inner workings of MVC, this method will not work if controllers are in 'AsServices' mode (via services.AddControllersAsServices())
            var mvcOptions = app.ApplicationServices.GetRequiredService<IOptions<MvcOptions>>();
            var restApiOptions = app.ApplicationServices.GetRequiredService<IOptions<RestApiOptions>>();
            mvcOptions.Value.Conventions.Add(new RestApiControllerRouteConvention(restApiOptions.Value, controllerRestInfoRepository));

            var applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.FeatureProviders.Add(new RestApiControllerFeatureProvider(controllerRestInfoRepository));

            return app;
        }
    }
}
