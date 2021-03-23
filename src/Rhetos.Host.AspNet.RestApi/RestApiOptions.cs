using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rhetos.Dsl;
using Rhetos.Host.AspNet.RestApi.Metadata;

namespace Rhetos.Host.AspNet.RestApi
{
    public class RestApiOptions
    {
        private static readonly IConceptInfoRestMetadataProvider[] _defaultMetadataProviders =
        {
            new ActionInfoRestMetadataProvider(),
            new ReportDataInfoRestMetadataProvider(),
            new DataStructureInfoRestMetadataProvider(),
        };

        public string BaseRoute { get; set; } = "RhetosRestApi";
        public List<IConceptInfoRestMetadataProvider> ConceptInfoRestMetadataProviders { get; set; } = new List<IConceptInfoRestMetadataProvider>(_defaultMetadataProviders);
        public Func<IConceptInfo, string, string> GroupNameMapper { get; set; }
    }
}
