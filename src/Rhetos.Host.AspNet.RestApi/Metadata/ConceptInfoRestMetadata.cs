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

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Rhetos.Dsl;
using Rhetos.Host.AspNet.RestApi.Controllers;
using System;

namespace Rhetos.Host.AspNet.RestApi.Metadata
{
    /// <summary>
    /// Inherit this class if you need to provide additional custom metadata from
    /// the concept metadata provider (implementation of <see cref="IConceptInfoRestMetadataProvider"/>)
    /// the controller (derivation of <see cref="RhetosApiControllerBase{T}"/>).
    /// </summary>
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
