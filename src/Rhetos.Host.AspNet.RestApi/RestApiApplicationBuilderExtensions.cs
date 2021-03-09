using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rhetos;
using Rhetos.Host.AspNet.RestApi;
using Rhetos.Host.AspNet.RestApi.Metadata;

namespace Microsoft.AspNetCore.Builder
{
    public static class RestApiApplicationBuilderExtensions
    {
        /// <summary>
        /// Add discovered Rhetos controllers to MVC feature pipeline and maps them to their respective routes.
        /// Call before 'app.UseRouting()'.
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
            // Due to this, it is important that this method is executed prior to any others that force feature enumeration (e.g. app.UseRouting())
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
