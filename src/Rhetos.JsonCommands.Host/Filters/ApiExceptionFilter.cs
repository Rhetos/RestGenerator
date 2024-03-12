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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Rhetos.JsonCommands.Host.Utilities;
using System;
using System.Linq;

namespace Rhetos.JsonCommands.Host.Filters
{
    /// <summary>
    /// Standard Rhetos REST error response format:
    /// In case of exception, the web response body will be an object with UserMessage and SystemMessage properties.
    /// </summary>
    /// <remarks>
    /// It also writes the exception to the application's log, based on severity:
    /// <see cref="UserException"/> is logged as Trace level (not logged by default), because it is expected during the standard app usage
    /// (for example, user forgot to enter a required field).
    /// <see cref="ClientException"/> is logged as Information level (logged by default), because it indicates that
    /// the client application needs to be corrected.
    /// Other exceptions are logged as Error level, because they represent internal error in the server application
    /// that needs to be fixed.
    /// </remarks>
    public class ApiExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ErrorReporting jsonErrorHandler;
        private readonly ILogger logger;

        public int Order { get; } = int.MaxValue - 10;

        public ApiExceptionFilter(ErrorReporting jsonErrorHandler, ILogger<ApiExceptionFilter> logger)
        {
            this.jsonErrorHandler = jsonErrorHandler;
            this.logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var invalidModelEntries = context.ModelState
                .Where(a => a.Value.ValidationState == ModelValidationState.Invalid)
                .ToList();

            if (!invalidModelEntries.Any())
                return;

            var invalidModelEntry = invalidModelEntries.First();
            var errors = string.Join("\n", invalidModelEntry.Value.Errors.Select(a => a.ErrorMessage));

            // If no key is present, it means there is an error deserializing body.

            if (string.IsNullOrEmpty(invalidModelEntry.Key))
            {
                var responseMessage = new ErrorReporting.ErrorResponse
                {
                    SystemMessage = "Serialization error: Please check if the request body has a valid JSON format.\n" + errors
                };
                context.Result = new JsonResult(responseMessage) { StatusCode = StatusCodes.Status400BadRequest };
            }
            else
            {
                var responseMessage = new ErrorReporting.ErrorResponse
                {
                    SystemMessage = $"Parameter error: Supplied value for parameter '{invalidModelEntry.Key}' couldn't be parsed.\n" + errors
                };
                context.Result = new JsonResult(responseMessage) { StatusCode = StatusCodes.Status400BadRequest };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                var error = jsonErrorHandler.CreateResponseFromException(context.Exception);

                context.Result = new JsonResult(error.Response) { StatusCode = error.StatusCode };
                context.ExceptionHandled = true;

                string commandSummerReport =
                    string.IsNullOrEmpty(error.CommandSummary) ? ""
                    : Environment.NewLine + "Command: " + error.CommandSummary;

                logger.Log(error.Severity, context.Exception.ToString() + commandSummerReport);
            }
        }
    }
}
