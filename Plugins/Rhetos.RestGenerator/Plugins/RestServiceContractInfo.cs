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

using Rhetos.Dsl;
using System.ComponentModel.Composition;

namespace Rhetos.RestGenerator.Plugins
{
    /// <summary>
    /// A helper concept for adding a *same* service contact name and service behavior name to many different services (usually one for each data structure).
    /// Same name will simplify overriding service binding configuration in Web.config for all services (for example, changing web request size limits, or setting HTTPS-only endpoint).
    /// </summary>
    [Export(typeof(IConceptInfo))]
    public class RestServiceContractInfo : IConceptInfo
    {
        /// <summary>
        /// Dependency to concept that is implemented by code generated that generated the service class.
        /// This will ensure that the service contract attribute is generated *after* the service class.
        /// </summary>
        [ConceptKey]
        public IConceptInfo ServiceGenerator { get; set; }

        [ConceptKey]
        public string ServiceClassAttributeTag { get; set; }
    }
}
