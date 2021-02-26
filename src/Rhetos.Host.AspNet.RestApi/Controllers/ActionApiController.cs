using System;
using Microsoft.AspNetCore.Mvc;
using Rhetos.Host.AspNet.RestApi.Utilities;

namespace Rhetos.Host.AspNet.RestApi.Controllers
{
    public class ActionApiController<T> : RhetosApiControllerBase<T>
    {
        private readonly ServiceUtility serviceUtility;

        public ActionApiController(ServiceUtility serviceUtility)
        {
            this.serviceUtility = serviceUtility;
        }

        [HttpPost]
        public void Execute([FromBody]T action)
        {
            serviceUtility.Execute(action);
        }
    }
}
