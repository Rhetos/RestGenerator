using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Rhetos.Host.AspNet;
using Rhetos.Persistence;

namespace Rhetos.Extensions.RestApi.Filters
{
    public class ApiCommitOnSuccessFilter : IActionFilter, IOrderedFilter
    {
        private readonly IRhetosComponent<IPersistenceTransaction> rhetosPersistenceTransaction;
        public int Order { get; } = int.MaxValue - 20;

        public ApiCommitOnSuccessFilter(IRhetosComponent<IPersistenceTransaction> rhetosPersistenceTransaction)
        {
            this.rhetosPersistenceTransaction = rhetosPersistenceTransaction;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Response.StatusCode == 200 && context.Exception == null)
            {
                rhetosPersistenceTransaction.Value.CommitChanges();
                rhetosPersistenceTransaction.Value.Dispose();
            }
        }
    }
}
