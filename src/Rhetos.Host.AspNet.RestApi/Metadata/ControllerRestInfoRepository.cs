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
                    restMetadata.ApiExplorerGroupName = _restApiOptions.Value.GroupNameMapper.Invoke(restMetadata.ConceptInfo, restMetadata.ApiExplorerGroupName);
            }

            return controllerConceptInfo;
        }
    }
}
