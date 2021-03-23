using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Rhetos;
using Rhetos.Dom;
using Rhetos.Host.AspNet;
using Rhetos.Host.AspNet.RestApi;
using Rhetos.Host.AspNet.RestApi.Filters;
using Rhetos.Host.AspNet.RestApi.Metadata;
using Rhetos.Host.AspNet.RestApi.Utilities;
using Rhetos.Persistence;
using Rhetos.Processing;
using Rhetos.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RhetosRestApiServiceCollectionExtensions
    {
        public static RhetosAspNetServiceCollectionBuilder AddRestApi(this RhetosAspNetServiceCollectionBuilder builder,
            Action<RestApiOptions> configureOptions = null)
        {
            builder.Services.AddOptions();

            if (configureOptions != null)
            {
                builder.Services.Configure<RestApiOptions>(configureOptions);
            }

            builder.Services.TryAddScoped<QueryParameters>();
            builder.Services.TryAddScoped<ServiceUtility>();
            builder.Services.TryAddScoped<JsonErrorHandler>();
            
            builder.Services.TryAddScoped<ApiExceptionFilter>();
            builder.Services.TryAddScoped<ApiCommitOnSuccessFilter>();
            builder.Services.TryAddSingleton<ControllerRestInfoRepository>();

            return builder;
        }
    }
}
