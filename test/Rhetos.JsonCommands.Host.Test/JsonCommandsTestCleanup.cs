using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.JsonCommands.Host.Test.Tools;
using System;
using TestApp;

namespace Rhetos.JsonCommands.Host.Test
{
    public class JsonCommandsTestCleanup : IDisposable
    {
        public void Dispose()
        {
            var factory = new CustomWebApplicationFactory<Startup>();
            using var scope = factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRhetosComponent<Common.DomRepository>>().Value;

            var testBooks = repository.Bookstore.Book.Load(book => book.Name.StartsWith("__Test__"));
            repository.Bookstore.Book.Delete(testBooks);

            scope.ServiceProvider.GetRequiredService<IRhetosComponent<IUnitOfWork>>().Value.CommitAndClose();
        }
    }
}