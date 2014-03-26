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

using Rhetos.Dom.DefaultConcepts;
using Rhetos.Logging;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Text;

namespace Rhetos.RestGenerator.Utilities
{
    public class ServiceLoader
    {
        private IProcessingEngine _processingEngine;
        private ILogger _logger;
        private ILogger _commandsLogger;
        private ILogger _performanceLogger;
        private Stopwatch _stopwatch;

        private static void CheckForErrors(ProcessingResult result)
        {
            if (!result.Success)
                throw new WebFaultException<MessagesResult>(
                        new MessagesResult { SystemMessage = result.SystemMessage, UserMessage = result.UserMessage }, 
                        string.IsNullOrEmpty(result.UserMessage) ? HttpStatusCode.InternalServerError : HttpStatusCode.BadRequest
                );
        }

        public ServiceLoader(
            Rhetos.Processing.IProcessingEngine processingEngine,
            Rhetos.Logging.ILogProvider logProvider) 
        {
            _processingEngine = processingEngine;
            _logger = logProvider.GetLogger("RestService");
            _commandsLogger = logProvider.GetLogger("RestService Commands");
            _performanceLogger = logProvider.GetLogger("Performance");
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.Trace("Rest Service loader initialized.");
        }

        public QueryResult<T> GetData<T>(object filter, Rhetos.Dom.DefaultConcepts.FilterCriteria[] genericFilter=null, int page=0, int psize=0, string sort="")
        {
            _performanceLogger.Write(_stopwatch, "RestService: GetData("+typeof (T).FullName+") Started.");
            _commandsLogger.Trace("GetData(" + typeof (T).FullName + ");");
            var commandInfo = new QueryDataSourceCommandInfo
                                  {
                                      DataSource = typeof (T).FullName,
                                      Filter = filter,
                                      GenericFilter = genericFilter,
                                      PageNumber = page,
                                      RecordsPerPage = psize
                                  };

            if (!String.IsNullOrWhiteSpace(sort))
            {
                var sortParameters = sort.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                commandInfo.OrderByProperty = sortParameters[0];
                commandInfo.OrderDescending = sortParameters.Count() >= 2 && sortParameters[1].ToLower().Equals("desc");
            }

            var result = _processingEngine.Execute(new[]{commandInfo});
            CheckForErrors(result);

            var resultData = (QueryDataSourceCommandResult)(((Rhetos.XmlSerialization.XmlBasicData<QueryDataSourceCommandResult>)(result.CommandResults.Single().Data)).Value);

            commandInfo.Filter = null;
            commandInfo.GenericFilter = null;

            _performanceLogger.Write(_stopwatch, "RestService: GetData("+typeof (T).FullName+") Executed.");
            return new QueryResult<T>
            {
                Records = resultData.Records.Select(o => (T)o).ToList(),
                TotalRecords = resultData.TotalRecords,
                CommandArguments = commandInfo
            };
        }

        private static T DeserializeJson<T>(string json)
        {
            return (T) DeserializeJson(typeof(T), json);
        }

        private static object DeserializeJson(Type type, string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(type);
                return serializer.ReadObject(stream);
            }
        }

        public static void GetFilterParameters(string filter, string fparam, string genericfilter, IDictionary<string, Type[]> filterTypesByName, out Rhetos.Dom.DefaultConcepts.FilterCriteria[] genericFilterInstance, out object filterInstance) 
        {
            if (!string.IsNullOrEmpty(genericfilter))
            {
                genericFilterInstance = DeserializeJson<Rhetos.Dom.DefaultConcepts.FilterCriteria[]>(genericfilter);
                if (genericFilterInstance == null)
                        throw new Rhetos.UserException("Invalid format of the generic filter: '" + genericfilter + "'.");
            }
            else
                genericFilterInstance = null;
           
            if (!string.IsNullOrEmpty(filter))
            {
                Type filterType = GetFilterType(filter, filterTypesByName);

                if (!string.IsNullOrEmpty(fparam))
                {
                    filterInstance = DeserializeJson(filterType, fparam);
                    if (filterInstance == null)
                        throw new Rhetos.UserException("Invalid filter parameter format for filter '" + filter + "', data: '" + fparam + "'.");
                }
                else
                    filterInstance = Activator.CreateInstance(filterType);
            }
            else
                filterInstance = null;
        }

        private static Type GetFilterType(string filterName, IDictionary<string, Type[]> filterTypesByName)
        {
            Type filterType = null;

            Type[] matchingTypes = null;
            filterTypesByName.TryGetValue(filterName, out matchingTypes);
            if (matchingTypes != null && matchingTypes.Count() > 1)
                throw new Rhetos.UserException("Filter type '" + filterName + "' is ambiguous (" + matchingTypes[0].FullName + ", " + matchingTypes[1].FullName + ").");
            if (matchingTypes != null && matchingTypes.Count() == 1)
                filterType = matchingTypes[0];

            if (filterType == null && Rhetos.Utilities.XmlUtility.Dom != null)
                filterType = Rhetos.Utilities.XmlUtility.Dom.GetType(filterName);

            if (filterType == null)
                filterType = Type.GetType(filterName);

            if (filterType == null)
                throw new Rhetos.UserException("Filter type '" + filterName + "' is not recognised.");

            return filterType;
        }

        public void Execute<T>(T action)
        {
            var commandInfo = new ExecuteActionCommandInfo { Action = action };
            var result = _processingEngine.Execute(new[] { commandInfo });
            CheckForErrors(result);
        }

        public ProcessingResult InsertData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToInsert = new[] { (IEntity)entity }
            };

            var result = _processingEngine.Execute(new[] { commandInfo });
            CheckForErrors(result);

            return result;
        }

        public ProcessingResult UpdateData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToUpdate = new[] { (IEntity)entity }
            };

            var result = _processingEngine.Execute(new[] { commandInfo });
            CheckForErrors(result);

            return result;
        }

        public ProcessingResult DeleteData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToDelete = new[] { (IEntity)entity }
            };

            var result = _processingEngine.Execute(new[] { commandInfo });
            CheckForErrors(result);

            return result;
        }
    }
}
