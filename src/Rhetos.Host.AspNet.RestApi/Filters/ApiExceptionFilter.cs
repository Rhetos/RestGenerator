using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Rhetos.Host.AspNet.RestApi.Utilities;

namespace Rhetos.Host.AspNet.RestApi.Filters
{
    public class ApiExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly JsonErrorHandler jsonErrorHandler;
        private readonly ILogger logger;

        public int Order { get; } = int.MaxValue - 10;

        public ApiExceptionFilter(JsonErrorHandler jsonErrorHandler, ILogger<ApiExceptionFilter> logger)
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
                var responseMessage = new JsonErrorHandler.ResponseMessage
                {
                    SystemMessage = "Serialization error: Please check if the request body has a valid JSON format.\n" + errors
                };
                context.Result = new JsonResult(responseMessage) {StatusCode = StatusCodes.Status400BadRequest};
            }
            else
            {
                var responseMessage = new JsonErrorHandler.ResponseMessage
                {
                    SystemMessage = $"Parameter error: Supplied value for parameter '{invalidModelEntry.Key}' couldn't be parsed.\n" + errors
                };
                context.Result = new JsonResult(responseMessage) {StatusCode = StatusCodes.Status400BadRequest};
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                var (response, statusCode) = jsonErrorHandler.CreateResponseFromException(context.Exception);
                context.Result = new JsonResult(response) { StatusCode = statusCode };
                context.ExceptionHandled = true;
                LogException(context.Exception);
            }
        }

        private void LogException(Exception error)
        {
            if (error is UserException)
                logger.LogTrace(error.ToString());
            else if (error is LegacyClientException legacyException)
            {
                if (legacyException.Severe)
                    logger.LogInformation(legacyException.ToString());
                else
                    logger.LogTrace(legacyException.ToString());
            }
            else if (error is ClientException)
                logger.LogInformation(error.ToString());
            else
                logger.LogError(error.ToString());
        }
    }
}
