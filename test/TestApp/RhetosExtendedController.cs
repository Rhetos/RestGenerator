using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rhetos;
using Rhetos.Host.AspNet.RestApi.Controllers;

namespace TestApp
{
    public class RhetosExtendedController<T> : RhetosApiControllerBase<T>
    {
        [HttpGet]
        [Route("ExtendedMethod")]
        public string ExtendedMethod()
        {
            return typeof(T).FullName;
        }

        public class DTInfo
        {
            public DateTime Time { get; set; }
        }
        [HttpPost]
        [Route("JsonDateTime")]
        public ActionResult<DTInfo> JsonAction([FromBody] DTInfo dtInfo)
        {
            var newDt = new DTInfo()
            {
                Time = dtInfo.Time.AddDays(1)
            };
            return new JsonResult(newDt);
        }

        [HttpPost]
        [Route("ByteArray")]
        public ActionResult<byte[]> ByteArrayAction([FromBody] byte[] bytes)
        {
            var newArray = bytes.Concat(new byte[] {42}).ToArray();
            return new JsonResult(newArray);
        }
    }
}
