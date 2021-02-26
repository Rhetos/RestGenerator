using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rhetos.Extensions.RestApi.Metadata
{
    public class DataStructureInfoMetadata : ConceptInfoRestMetadata
    {
        private readonly Tuple<string, Type>[] parameters;
        public DataStructureInfoMetadata(IEnumerable<Tuple<string, Type>> parameters)
        {
            this.parameters = parameters.ToArray();
        }

        public Tuple<string, Type>[] GetParameters()
        {
            return parameters;
        }
    }
}
