using System;
using System.Collections.Generic;
using Rhetos.Dsl;

namespace Rhetos.Extensions.RestApi.Metadata
{
    public interface IConceptInfoRestMetadataProvider
    {
        IEnumerable<ConceptInfoRestMetadata> GetConceptInfoRestMetadata(RhetosHost rhetosHost);
    }
}
