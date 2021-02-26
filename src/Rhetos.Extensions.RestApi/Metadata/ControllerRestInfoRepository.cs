using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rhetos.Extensions.RestApi.Metadata
{
    public class ControllerRestInfoRepository
    {
        public readonly Dictionary<Type, ConceptInfoRestMetadata> ControllerConceptInfo = new Dictionary<Type, ConceptInfoRestMetadata>();
    }
}
