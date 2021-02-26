using System;
using System.Linq;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Controllers;
using Rhetos.Host.AspNet.RestApi.Metadata;

namespace Rhetos.Host.AspNet.RestApi
{
    public class RestApiControllerRouteConvention : IControllerModelConvention
    {
        private readonly RestApiOptions restApiOptions;
        private readonly ControllerRestInfoRepository controllerRestInfoRepository;

        public RestApiControllerRouteConvention(RestApiOptions restApiOptions, ControllerRestInfoRepository controllerRestInfoRepository)
        {
            this.restApiOptions = restApiOptions;
            this.controllerRestInfoRepository = controllerRestInfoRepository;
        }

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsClosedTypeOf(typeof(RhetosApiControllerBase<>)))
            {
                var restMetadata = controllerRestInfoRepository.ControllerConceptInfo[controller.ControllerType.AsType()];
                var route = $"{restApiOptions.BaseRoute}/{restMetadata.RelativeRoute}";
                controller.ControllerName = restMetadata.ControllerName;
                controller.ApiExplorer.GroupName = restMetadata.ApiExplorerGroupName;
                controller.ApiExplorer.IsVisible = restMetadata.IsVisible;

                controller.Selectors.Add(new SelectorModel()
                {
                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(route))
                });
            }
        }
    }
}
