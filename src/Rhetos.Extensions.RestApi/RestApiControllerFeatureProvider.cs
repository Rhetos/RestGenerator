using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Rhetos.Extensions.RestApi.Metadata;

namespace Rhetos.Extensions.RestApi
{
    public class RestApiControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly ControllerRestInfoRepository controllerRestInfoRepository;

        public RestApiControllerFeatureProvider(ControllerRestInfoRepository controllerRestInfoRepository)
        {
            this.controllerRestInfoRepository = controllerRestInfoRepository;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var restItem in controllerRestInfoRepository.ControllerConceptInfo.Values)
            {
                feature.Controllers.Add(restItem.ControllerType.GetTypeInfo());
            }
        }
    }
}
