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
using Microsoft.Extensions.Options;

namespace Rhetos.Host.AspNet.RestApi.Metadata
{
    public class ControllerRestInfoRepository
    {
        public Dictionary<Type, ConceptInfoRestMetadata> ControllerConceptInfo => _controllerConceptInfo.Value;

        private readonly IOptions<RestApiOptions> _restApiOptions;
        private readonly RhetosHost _rhetosHost;
        private readonly Lazy<Dictionary<Type, ConceptInfoRestMetadata>> _controllerConceptInfo;

        public ControllerRestInfoRepository(IOptions<RestApiOptions> restApiOptions, RhetosHost rhetosHost)
        {
            _restApiOptions = restApiOptions;
            _rhetosHost = rhetosHost;
            _controllerConceptInfo = new Lazy<Dictionary<Type, ConceptInfoRestMetadata>>(CreateControllerRestInfoRepository);
        }

        private Dictionary<Type, ConceptInfoRestMetadata> CreateControllerRestInfoRepository()
        {
            var controllerConceptInfo = new Dictionary<Type, ConceptInfoRestMetadata>();
            foreach (var conceptInfoRestMetadataProvider in _restApiOptions.Value.ConceptInfoRestMetadataProviders)
            {
                var metadataFromProvider = conceptInfoRestMetadataProvider.GetConceptInfoRestMetadata(_rhetosHost);
                foreach (var metadataItem in metadataFromProvider)
                    controllerConceptInfo.Add(metadataItem.ControllerType, metadataItem);
            }

            // transform all group names
            if (_restApiOptions.Value.GroupNameMapper != null)
            {
                foreach (var restMetadata in controllerConceptInfo.Values)
                    restMetadata.ApiExplorerGroupName = _restApiOptions.Value.GroupNameMapper.Invoke(restMetadata.ConceptInfo, restMetadata.ControllerType, restMetadata.ApiExplorerGroupName);
            }

            return controllerConceptInfo;
        }
    }
}
