using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rhetos.Extensions.RestApi.Filters;

namespace Rhetos.Extensions.RestApi.Controllers
{
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [ServiceFilter(typeof(ApiCommitOnSuccessFilter))]
    public class RhetosApiControllerBase<T> : ControllerBase
    {
    }
}
