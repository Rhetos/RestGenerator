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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rhetos.Utilities;
using System;

namespace Rhetos.Host.AspNet.RestApi.Utilities
{
    /// <summary>
    /// Converts exceptions to a HTTP WEB response that contains JSON-serialized string error message.
    /// </summary>
    public class ErrorReporting
    {
        private readonly ILocalizer localizer;

        public class ErrorResponse
        {
            public string UserMessage { get;set; }
            public string SystemMessage { get; set; }
            public override string ToString()
            {
                return "SystemMessage: " + (SystemMessage ?? "<null>") + ", UserMessage: " + (UserMessage ?? "<null>");
            }
        }

        public ErrorReporting(IRhetosComponent<ILocalizer> rhetosLocalizer)
        {
            this.localizer = rhetosLocalizer.Value;
        }

        public ErrorDescription CreateResponseFromException(Exception error)
        {
            string commandSummary = ExceptionsUtility.GetCommandSummary(error);

            if (error is UserException userException)
            {
                return new ErrorDescription(
                    StatusCodes.Status400BadRequest,
                    new ErrorResponse
                    {
                        UserMessage = localizer[userException.UserMessage, userException.MessageParameters],
                        SystemMessage = userException.SystemMessage
                    },
                    LogLevel.Trace,
                    commandSummary);
            }
            else if (error is LegacyClientException legacyClientException)
            {
                return new ErrorDescription(
                    (int)legacyClientException.HttpStatusCode,
                    legacyClientException.Message,
                    legacyClientException.Severe ? LogLevel.Information : LogLevel.Trace,
                    commandSummary);
            }
            else if (error is ClientException clientException)
            {
                int statusCode = GetStatusCode(clientException);
                return new ErrorDescription(
                    statusCode,
                    new ErrorResponse
                    {
                        UserMessage = statusCode == (int)System.Net.HttpStatusCode.BadRequest
                            ? localizer[ErrorMessages.ClientExceptionUserMessage]
                            // ClientExceptionUserMessage is intended for invalid request format (default). Other errors are not correctly described with that message.
                            : clientException.Message, // This is compatible with v4 behavior, but this might be a breaking change for v5.0 apps.
                        SystemMessage = clientException.Message
                    },
                    LogLevel.Information,
                    commandSummary);
            }
            else
            {
                return new ErrorDescription(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse { SystemMessage = ErrorMessages.GetInternalServerErrorMessage(localizer, error) },
                    LogLevel.Error,
                    commandSummary);
            }
        }

        private int GetStatusCode(ClientException clientException)
        {
            // HACK: Old Rhetos plugins could not specify the status code. Here we match by message convention.
            if (clientException.Message == "User is not authenticated." && (int)clientException.HttpStatusCode == StatusCodes.Status400BadRequest)
                return StatusCodes.Status401Unauthorized;
            else
                return (int)clientException.HttpStatusCode;
        }
    }

    public class ErrorDescription
    {
        /// <summary>HTTP response status code.</summary>
        public int StatusCode { get; }

        public object Response { get; }

        public LogLevel Severity { get; }

        public string CommandSummary { get; }

        public ErrorDescription(int statusCode, object response, LogLevel severity, string commandSummary)
        {
            this.StatusCode = statusCode;
            this.Response = response;
            this.Severity = severity;
            this.CommandSummary = commandSummary;
        }
    }
}
