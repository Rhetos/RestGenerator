using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rhetos.Host.AspNet.RestApi.Filters;

namespace Rhetos.Host.AspNet.RestApi.Controllers
{
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [ServiceFilter(typeof(ApiCommitOnSuccessFilter))]
    public class RhetosApiControllerBase<T> : ControllerBase
    {
    }
}
