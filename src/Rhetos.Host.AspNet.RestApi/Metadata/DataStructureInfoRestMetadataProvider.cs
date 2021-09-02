/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
                var readParameters = dataStructureReadParameters.GetReadParameters(dataStructureInfo.FullName, true)
                    .Select(parameter => Tuple.Create(parameter.Name, parameter.Type))
                    .ToArray();

                var dataStructureInfoMetadata = new DataStructureInfoMetadata
                {
                    ReadParameters = readParameters,
                    ConceptInfo = dataStructureInfo,
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
