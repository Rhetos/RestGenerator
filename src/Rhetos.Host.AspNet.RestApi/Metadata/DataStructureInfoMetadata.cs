using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rhetos.Host.AspNet.RestApi.Metadata
{
    public class DataStructureInfoMetadata : ConceptInfoRestMetadata
    {
        /// <summary>
        /// Filter types and other parameter types, that are supported on this data structure's controller for reading the data.
        /// </summary>
        public Tuple<string, Type>[] ReadParameters { get; set; }
    }
}
