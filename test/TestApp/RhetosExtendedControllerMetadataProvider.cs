using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Rhetos;
using Rhetos.Dom;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Metadata;

namespace TestApp
{
    public class RhetosExtendedControllerMetadataProvider : IConceptInfoRestMetadataProvider
    {
        public IEnumerable<ConceptInfoRestMetadata> GetConceptInfoRestMetadata(RhetosHost rhetosHost)
        {
            var dslModel = rhetosHost.GetRootContainer().Resolve<IDslModel>();
            var domainObjectModel = rhetosHost.GetRootContainer().Resolve<IDomainObjectModel>();

            var restMetadata = dslModel
                .FindByType<DataStructureInfo>()
                .Where(dataStructureInfo => dataStructureInfo.Module.Name == "AspNetDemo")
                .Select(dataStructureInfo => new ConceptInfoRestMetadata()
                {
                    ControllerType = typeof(RhetosExtendedController<>).MakeGenericType(domainObjectModel.GetType($"{dataStructureInfo.FullName}")),
                    ControllerName = $"{dataStructureInfo.Module.Name}.{dataStructureInfo.Name}",
                    RelativeRoute = $"{dataStructureInfo.Module.Name}/{dataStructureInfo.Name}",
                    ApiExplorerGroupName = dataStructureInfo.Module.Name,
                });

            return restMetadata;
        }
    }
}
