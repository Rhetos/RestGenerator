using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Rhetos;
using Rhetos.Dom;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Filters;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhetos.Host.AspNet.RestApi.Controllers
{
    /// <summary>
    /// Web API za snimanje više zapisa odjednom.
    /// Omogućuje da se u jednom web requestu (i u jednoj db transakciji) odjednom inserta, deletea i updatea više zapisa od više različitih entiteta.
    /// Primjer JSON formata za podatke koje treba poslati je opisan u ovom komentaru: https://github.com/Rhetos/Rhetos/issues/355#issuecomment-915180224
    /// </summary>
    [Route("jc")]
    [ApiController]
    [ServiceFilter(typeof(ApiCommitOnSuccessFilter))]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    public class JsonCommandsController : ControllerBase
    {
        private readonly IDomainObjectModel _dom;
        private readonly IProcessingEngine _processingEngine;

        public JsonCommandsController(IRhetosComponent<IDomainObjectModel> dom, IRhetosComponent<IProcessingEngine> processingEngine)
        {
            _dom = dom.Value;
            _processingEngine = processingEngine.Value;
        }

        [HttpPost("write")]
        public IActionResult Write(List<Dictionary<string, JObject>> commands)
        {
            foreach (var commandDict in commands)
            {
                var command = commandDict.Single(); // Each command is deserialized as a dictionary to simplify the code, but only one key-value pair is allowed.
                string entityName = command.Key;
                Type entityType = _dom.GetType(entityName);
                Type itemsType = typeof(WriteCommandItems<>).MakeGenericType(entityType);
                dynamic items = command.Value.ToObject(itemsType);

                var saveEntityCommand = new SaveEntityCommandInfo
                {
                    Entity = entityName,
                    DataToDelete = items.Delete,
                    DataToUpdate = items.Update,
                    DataToInsert = items.Insert
                };
                _processingEngine.Execute(saveEntityCommand);
            }
            return Ok();
        }

        private class WriteCommandItems<T> where T : IEntity
        {
            public T[] Delete { get; set; }
            public T[] Update { get; set; }
            public T[] Insert { get; set; }
        }
    }
}
