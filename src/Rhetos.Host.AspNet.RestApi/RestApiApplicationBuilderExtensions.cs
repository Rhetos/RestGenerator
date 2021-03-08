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
        public static IApplicationBuilder UseRhetosRestApi(this IApplicationBuilder app)
        {
            var controllerRestInfoRepository = app.ApplicationServices.GetService<ControllerRestInfoRepository>();

            if (controllerRestInfoRepository == null)
            {
                throw new InvalidOperationException(
                    $"Can't resolve {nameof(ControllerRestInfoRepository)} from dependency injection. Did you forget to 'AddRhetos().AddRestApi()' in ConfigureServices of Startup.cs file?");
            }

            var mvcOptions = app.ApplicationServices.GetRequiredService<IOptions<MvcOptions>>();
            var restApiOptions = app.ApplicationServices.GetRequiredService<IOptions<RestApiOptions>>();
            mvcOptions.Value.Conventions.Add(new RestApiControllerRouteConvention(restApiOptions.Value, controllerRestInfoRepository));

            var applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.FeatureProviders.Add(new RestApiControllerFeatureProvider(controllerRestInfoRepository));

            return app;
        }
    }
}
