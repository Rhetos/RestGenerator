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
                .Select(dataStructureInfo => new ConceptInfoRestMetadata
                {
                    ConceptInfo = dataStructureInfo,
                    ControllerType = typeof(RhetosExtendedController<>).MakeGenericType(domainObjectModel.GetType($"{dataStructureInfo.FullName}")),
                    ControllerName = $"{dataStructureInfo.Module.Name}.{dataStructureInfo.Name}",
                    RelativeRoute = $"{dataStructureInfo.Module.Name}/{dataStructureInfo.Name}",
                    ApiExplorerGroupName = dataStructureInfo.Module.Name,
                });

            return restMetadata;
        }
    }
}
