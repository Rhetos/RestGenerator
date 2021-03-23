using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Rhetos.Dsl;
using Rhetos.Host.AspNet.RestApi.Metadata;

namespace Rhetos.Host.AspNet.RestApi
{
    /// <summary>
    /// Options class for Rhetos REST API.
    /// </summary>
    /// <remarks>
    /// It is intended by be configured within Startup.ConfigureServices method, by a delegate parameter of <see cref="RhetosRestApiServiceCollectionExtensions.AddRestApi"/> method call.
    /// </remarks>
    public class RestApiOptions
    {
        private static readonly IConceptInfoRestMetadataProvider[] _defaultMetadataProviders =
        {
            new ActionInfoRestMetadataProvider(),
            new ReportDataInfoRestMetadataProvider(),
            new DataStructureInfoRestMetadataProvider(),
        };

        /// <summary>
        /// Base route for REST API.
        /// </summary>
        public string BaseRoute { get; set; } = "rest";

        /// <summary>
        /// List of controller providers.
        /// By default, it includes providers for DSL concepts DataStructure, Action and ReportData (including derived concepts).
        /// It can be extended with custom provider.
        /// </summary>
        public List<IConceptInfoRestMetadataProvider> ConceptInfoRestMetadataProviders { get; set; } = new List<IConceptInfoRestMetadataProvider>(_defaultMetadataProviders);

        /// <summary>
        /// Overrides ApiExplorer group names, initially provided by <see cref="IConceptInfoRestMetadataProvider"/>.
        /// Initial group names are typically module names.
        /// </summary>
        public Func<IConceptInfo, string, string> GroupNameMapper { get; set; }
    }
}
