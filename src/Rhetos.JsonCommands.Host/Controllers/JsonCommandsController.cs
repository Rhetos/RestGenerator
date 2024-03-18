using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Rhetos;
using Rhetos.Dom;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.JsonCommands.Host.Filters;
using Rhetos.JsonCommands.Host.Utilities;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhetos.JsonCommands.Host.Controllers
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
        private readonly IRhetosComponent<GenericFilterHelper> _genericFilterHelper;

        public JsonCommandsController(IRhetosComponent<IDomainObjectModel> dom, IRhetosComponent<IProcessingEngine> processingEngine, IRhetosComponent<GenericFilterHelper> genericFilterHelper)
        {
            _dom = dom.Value;
            _processingEngine = processingEngine.Value;
            _genericFilterHelper = genericFilterHelper;
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

        [HttpPost("read")]
        public IActionResult Read(List<Dictionary<string, JObject>> commands)
        {
            List<ReadCommandResult> results = new List<ReadCommandResult>();
            foreach (var commandDict in commands)
            {
                var command = commandDict.Single(); // Each command is deserialized as a dictionary to simplify the code, but only one key-value pair is allowed.
                string entityName = command.Key;
                Type entityType = _dom.GetType(entityName);
                JObject properties = command.Value;

                bool readRecords = properties.GetValue("ReadRecords").Value<bool>();
                bool readTotalCount = properties.GetValue("ReadTotalCount").Value<bool>();

                bool hasFilter = properties.TryGetValue("Filters", out var filters);
                bool hasSort = properties.TryGetValue("Sort", out var sort);
                bool hasSkip = properties.TryGetValue("Skip", out var skip);
                bool hasTop = properties.TryGetValue("Top", out var top);

                var readEntityCommand = new ReadCommandInfo
                {
                    DataSource = entityName,
                    Filters = hasFilter
                        ? new QueryParameters(_genericFilterHelper)
                            .ParseFilterParameters(filters.ToString(), entityName)
                        : Array.Empty<FilterCriteria>(),
                    OrderByProperties = hasSort
                        ? sort
                            .Values<string>()
                            .Select((e) => new OrderByProperty() { 
                                Property = e.StartsWith('-') ? e.Substring(1) : e,
                                Descending = e.StartsWith('-')
                            })
                            .ToArray()
                        : Array.Empty<OrderByProperty>(),
                    ReadRecords = readRecords,
                    ReadTotalCount = readTotalCount,
                    Skip = hasSkip ? skip.Value<int>() : 0,
                    Top = hasTop ? top.Value<int>() : 0,
                };
                results.Add(_processingEngine.Execute(readEntityCommand));
            }
            return Ok(results);
        }

        private class WriteCommandItems<T> where T : IEntity
        {
            public T[] Delete { get; set; }
            public T[] Update { get; set; }
            public T[] Insert { get; set; }
        }
    }
}
