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
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Rhetos.Dsl;
using Rhetos.Host.AspNet.RestApi.Metadata;

namespace Rhetos.Host.AspNet.RestApi
{
    /// <summary>
    /// Options class for Rhetos REST API.
    /// </summary>
    /// <remarks>
    /// It is intended by be configured within Startup.ConfigureServices method, by a delegate parameter of <see cref="RhetosRestApiServiceCollectionExtensions.AddRestApi"/> method call.
    /// </remarks>
    public class RestApiOptions
    {
        private static readonly IConceptInfoRestMetadataProvider[] _defaultMetadataProviders =
        {
        };

        /// <summary>
        /// Base route for REST API.
        /// </summary>
        public string BaseRoute { get; set; } = "rest";

        /// <summary>
        /// List of controller providers.
        /// By default, it includes providers for DSL concepts DataStructure, Action and ReportData (including derived concepts).
        /// It can be extended with custom provider.
        /// </summary>
        public List<IConceptInfoRestMetadataProvider> ConceptInfoRestMetadataProviders { get; set; } = new List<IConceptInfoRestMetadataProvider>(_defaultMetadataProviders);

        /// <summary>
        /// Overrides ApiExplorer group names, initially provided by <see cref="IConceptInfoRestMetadataProvider"/>.
        /// Initial group names are typically module names.
        /// </summary>
        public Func<IConceptInfo, Type, string, string> GroupNameMapper { get; set; }
    }
}
