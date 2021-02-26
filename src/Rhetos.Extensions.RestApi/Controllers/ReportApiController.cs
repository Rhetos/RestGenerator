using System;
using Microsoft.AspNetCore.Mvc;
using Rhetos.Extensions.RestApi.Utilities;

namespace Rhetos.Extensions.RestApi.Controllers
{
    public class ReportApiController<T> : RhetosApiControllerBase<T>
    {
        private readonly ServiceUtility serviceUtility;

        public ReportApiController(ServiceUtility serviceUtility)
        {
            this.serviceUtility = serviceUtility;
        }

        [HttpGet]
        public ActionResult<DownloadReportResult> DownloadReport(string parameter, string convertFormat)
        {
            var reportResult = serviceUtility.DownloadReport<T>(parameter, convertFormat);
            return new JsonResult(reportResult);
        }
    }
}
