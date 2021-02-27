using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Rhetos.Dom;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Controllers;

namespace Rhetos.Host.AspNet.RestApi.Metadata
{
    public class DataStructureInfoRestMetadataProvider : IConceptInfoRestMetadataProvider
    {
        public IEnumerable<ConceptInfoRestMetadata> GetConceptInfoRestMetadata(RhetosHost rhetosHost)
        {
            var dslModel = rhetosHost.GetRootContainer().Resolve<IDslModel>();
            var domainObjectModel = rhetosHost.GetRootContainer().Resolve<IDomainObjectModel>();
            var dataStructureReadParameters = rhetosHost.GetRootContainer().Resolve<IDataStructureReadParameters>();

            DataStructureInfoMetadata CreateFromGenericController(Type genericControllerType, DataStructureInfo dataStructureInfo)
            {
                var parameters = dataStructureReadParameters.GetReadParameters(dataStructureInfo.FullName, true)
                    .Select(parameter => Tuple.Create(parameter.Name, parameter.Type))
                    .ToArray();

                var dataStructureInfoMetadata = new DataStructureInfoMetadata(parameters)
                {
                    ControllerType = genericControllerType.MakeGenericType(domainObjectModel.GetType($"{dataStructureInfo.FullName}")),
                    ControllerName = $"{dataStructureInfo.Module.Name}.{dataStructureInfo.Name}",
                    RelativeRoute = $"{dataStructureInfo.Module.Name}/{dataStructureInfo.Name}",
                    ApiExplorerGroupName = dataStructureInfo.Module.Name,
                };

                return dataStructureInfoMetadata;
            }

            var dataStructuresByWriteInfo = dslModel
                .FindByType<WriteInfo>()
                .Select(writeInfo => writeInfo.DataStructure)
                .Distinct()
                .ToHashSet();

            Type DataStructureControllerType(DataStructureInfo dataStructureInfo)
            {
                if (dataStructureInfo is IWritableOrmDataStructure || dataStructuresByWriteInfo.Contains(dataStructureInfo))
                    return typeof(ReadWriteDataApiController<>);
                else if (IsDataStructureTypeSupported(dataStructureInfo))
                    return typeof(ReadDataApiController<>);

                return null;
            }

            var restMetadata = dslModel
                .FindByType<DataStructureInfo>()
                .Select(dataStructureInfo => (dataStructureInfo, controllerType: DataStructureControllerType(dataStructureInfo)))
                .Where(implementation => implementation.controllerType != null)
                .Select(implementation => CreateFromGenericController(implementation.controllerType, implementation.dataStructureInfo));

            return restMetadata;
        }

        private static bool IsDataStructureTypeSupported(DataStructureInfo conceptInfo)
        {
            return conceptInfo is IOrmDataStructure
                   || conceptInfo is BrowseDataStructureInfo
                   || conceptInfo is QueryableExtensionInfo
                   || conceptInfo is ComputedInfo;
        }
    }
}
