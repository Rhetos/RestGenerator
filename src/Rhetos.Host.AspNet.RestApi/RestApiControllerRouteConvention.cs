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
                controller.ApiExplorer.IsVisible = restMetadata.ApiExplorerIsVisible;

                controller.Selectors.Add(new SelectorModel()
                {
                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(route))
                });
            }
        }
    }
}
