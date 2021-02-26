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
        public string BaseRoute { get; set; }
        public List<IConceptInfoRestMetadataProvider> ConceptInfoRestMetadataProviders { get; set; }
        public Func<IConceptInfo, string, string> GroupNameMapper { get; set; }
    }
}
