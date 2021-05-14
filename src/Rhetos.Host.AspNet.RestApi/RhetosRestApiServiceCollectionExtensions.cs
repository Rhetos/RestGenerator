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
        /// Adds Rhetos sepcific filters into the ASP.NET request processing pipeline.
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
