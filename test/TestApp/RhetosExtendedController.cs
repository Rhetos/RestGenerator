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
