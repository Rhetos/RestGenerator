using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Rhetos.Extensions.RestApi.Utilities;

namespace Rhetos.Extensions.RestApi.Filters
{
    public class ApiExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly JsonErrorHandler jsonErrorHandler;
        public int Order { get; } = int.MaxValue - 10;

        public ApiExceptionFilter(JsonErrorHandler jsonErrorHandler)
        {
            this.jsonErrorHandler = jsonErrorHandler;
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

            // if no key is present, it means there is an error deserializing body
            
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
            }
        }
    }
}
