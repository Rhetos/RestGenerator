using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Rhetos.Dsl;

namespace Rhetos.Host.AspNet.RestApi.Metadata
{
    public class ConceptInfoRestMetadata
    {
        public IConceptInfo ConceptInfo { get; set; }

        public Type ControllerType { get; set; }

        public string ControllerName { get; set; }

        public string RelativeRoute { get; set; }

        /// <summary>
        /// Group name in API explorer (for example Swagger UI).
        /// See <see cref="ApiExplorerModel.GroupName"/>.
        /// </summary>
        public string ApiExplorerGroupName { get; set; }

        /// <summary>
        /// Visibility in API explorer (for example Swagger UI).
        /// See <see cref="ApiExplorerModel.IsVisible"/>
        /// </summary>
        public bool ApiExplorerIsVisible { get; set; } = true;
    }
}
