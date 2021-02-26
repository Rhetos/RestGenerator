using System;
using System.Collections.Generic;
using Rhetos.Dsl;

namespace Rhetos.Host.AspNet.RestApi.Metadata
{
    public interface IConceptInfoRestMetadataProvider
    {
        IEnumerable<ConceptInfoRestMetadata> GetConceptInfoRestMetadata(RhetosHost rhetosHost);
    }
}
