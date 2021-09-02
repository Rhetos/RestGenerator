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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Rhetos.Utilities;

namespace Rhetos.Host.AspNet.RestApi.Utilities
{
    /// <summary>
    /// Converts exceptions to a HTTP WEB response that contains JSON-serialized string error message.
    /// </summary>
    public class JsonErrorHandler
    {
        private readonly ILocalizer localizer;

        public class ResponseMessage
        {
            public string UserMessage { get;set; }
            public string SystemMessage { get; set; }
            public override string ToString()
            {
                return "SystemMessage: " + (SystemMessage ?? "<null>") + ", UserMessage: " + (UserMessage ?? "<null>");
            }
        }

        public JsonErrorHandler(IRhetosComponent<ILocalizer> rhetosLocalizer)
        {
            this.localizer = rhetosLocalizer.Value;
        }

        public (object response, int statusCode) CreateResponseFromException(Exception error)
        {
            object responseMessage;
            int responseStatusCode;

            if (error is UserException userException)
            {
                responseStatusCode = StatusCodes.Status400BadRequest;
                responseMessage = new ResponseMessage
                {
                    UserMessage = localizer[userException.Message, userException.MessageParameters],
                    SystemMessage = userException.SystemMessage
                };
            }
            else if (error is LegacyClientException legacyClientException)
            {
                responseStatusCode = (int)legacyClientException.HttpStatusCode;
                responseMessage = legacyClientException.Message;
            }
            else if (error is ClientException clientException)
            {
                responseStatusCode = (int)clientException.HttpStatusCode;
                responseMessage = new ResponseMessage { SystemMessage = clientException.Message };

                // HACK: Old Rhetos plugins could not specify the status code. Here we match by message convention.
                if (clientException.Message == "User is not authenticated." && responseStatusCode == StatusCodes.Status400BadRequest)
                    responseStatusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                responseStatusCode = StatusCodes.Status500InternalServerError;
                responseMessage = new ResponseMessage { SystemMessage = ErrorReporting.GetInternalServerErrorMessage(localizer, error) };
            }

            return (responseMessage, responseStatusCode);
        }
    }
}
