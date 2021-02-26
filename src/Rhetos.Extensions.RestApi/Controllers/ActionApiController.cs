using System;
using Microsoft.AspNetCore.Mvc;
using Rhetos.Extensions.RestApi.Utilities;

namespace Rhetos.Extensions.RestApi.Controllers
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
