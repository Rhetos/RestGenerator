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
