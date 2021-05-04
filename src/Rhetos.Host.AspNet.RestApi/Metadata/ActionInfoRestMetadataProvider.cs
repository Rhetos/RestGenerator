using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Rhetos.Dom;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Controllers;

namespace Rhetos.Host.AspNet.RestApi.Metadata
{
    public class ActionInfoRestMetadataProvider : IConceptInfoRestMetadataProvider
    {
        public IEnumerable<ConceptInfoRestMetadata> GetConceptInfoRestMetadata(RhetosHost rhetosHost)
        {
            var dslModel = rhetosHost.GetRootContainer().Resolve<IDslModel>();
            var domainObjectModel = rhetosHost.GetRootContainer().Resolve<IDomainObjectModel>();

            var restMetadata = dslModel
                .FindByType<ActionInfo>()
                .Select(actionInfo => new ConceptInfoRestMetadata
                {
                    ConceptInfo = actionInfo,
                    ControllerType = typeof(ActionApiController<>).MakeGenericType(domainObjectModel.GetType($"{actionInfo.FullName}")),
                    ControllerName = $"{actionInfo.Module.Name}.{actionInfo.Name}",
                    RelativeRoute = $"{actionInfo.Module.Name}/{actionInfo.Name}",
                    ApiExplorerGroupName = actionInfo.Module.Name,
                });

            return restMetadata;
        }
    }
}
