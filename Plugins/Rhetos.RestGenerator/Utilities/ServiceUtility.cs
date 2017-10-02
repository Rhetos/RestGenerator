﻿/*
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhetos.Dom;
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
using System.ServiceModel.Web;
using System.Text;

namespace Rhetos.RestGenerator.Utilities
{
    public class ServiceUtility
    {
        private IProcessingEngine _processingEngine;
        private ILogger _logger;
        private ILogger _commandsLogger;
        private ILogger _performanceLogger;
        private IDomainObjectModel _domainObjectModel;

        private static void CheckForErrors(ProcessingResult result)
        {
            if (!result.Success)
            {
                // TODO: Remove this method after simplifying ProcessingEngine error handling to always throw exceptions on error.
                if (result.UserMessage != null)
                    throw new UserException(result.UserMessage, result.SystemMessage); // JsonErrorHandler will return HttpStatusCode.BadRequest.
                throw new FrameworkException(result.SystemMessage); // JsonErrorHandler will return HttpStatusCode.InternalServerError.
            }
        }

        public ServiceUtility(
            IProcessingEngine processingEngine,
            ILogProvider logProvider,
            IDomainObjectModel domainObjectModel)
        {
            _processingEngine = processingEngine;
            _logger = logProvider.GetLogger("RestService");
            _commandsLogger = logProvider.GetLogger("RestService Commands");
            _performanceLogger = logProvider.GetLogger("Performance");
            _logger.Trace("Rest Service loader initialized.");
            _domainObjectModel = domainObjectModel;
        }

        public RecordsAndTotalCountResult<T> GetData<T>(string filter, string fparam, string genericfilter, string filters, IDictionary<string, Type[]> filterTypesByName, int top, int skip, int page, int psize, string sort, bool readRecords, bool readTotalCount)
        {
            // Legacy interface:
            if (page != 0 || psize != 0)
            {
                if (top != 0 || skip != 0)
                    throw new ClientException("Invalid paging parameter: Use either 'top' and 'skip', or 'page' and 'psize'.");

                top = psize;
                skip = page > 0 ? psize * (page - 1) : 0;
            }

            var readCommandInfo = new ReadCommandInfo
            {
                DataSource = typeof(T).FullName,
                Filters = ParseFilterParameters(filter, fparam, genericfilter, filters, filterTypesByName),
                Top = top,
                Skip = skip,
                ReadRecords = readRecords,
                ReadTotalCount = readTotalCount,
                OrderByProperties = ParseSortParameter(sort)
            };

            var readCommandResult = ExecuteReadCommand(readCommandInfo);

            return new RecordsAndTotalCountResult<T>
            {
                Records = readCommandResult.Records != null ? readCommandResult.Records.Cast<T>().ToArray() : null,
                TotalCount = readCommandResult.TotalCount != null ? readCommandResult.TotalCount.Value : 0
            };
        }

        public T GetDataById<T>(string idString)
        {
            Guid id;
            if (!Guid.TryParse(idString, out id))
                throw new LegacyClientException("Invalid format of GUID parametar 'ID'.");

            var filterInstance = new[] { id };

            return (T)ExecuteReadCommand(new ReadCommandInfo
            {
                DataSource = typeof(T).FullName,
                Filters = new[] { new FilterCriteria { Filter = filterInstance.GetType().AssemblyQualifiedName, Value = filterInstance } },
                ReadRecords = true
            })
                .Records.FirstOrDefault();
        }

        private ReadCommandResult ExecuteReadCommand(ReadCommandInfo commandInfo)
        {
            var sw = Stopwatch.StartNew();

            var result = _processingEngine.Execute(new[] { commandInfo });
            CheckForErrors(result);
            var resultData = (ReadCommandResult)(((Rhetos.XmlSerialization.XmlBasicData<ReadCommandResult>)(result.CommandResults.Single().Data)).Value);

            _performanceLogger.Write(sw, "RestService: ExecuteReadCommand(" + commandInfo.DataSource + ") Executed.");
            return resultData;
        }

        private OrderByProperty[] ParseSortParameter(string sort)
        {
            var result = new List<OrderByProperty>();

            if (!String.IsNullOrWhiteSpace(sort))
            {
                var properties = sort.Split(',').Select(sp => sp.Trim()).Where(sp => !string.IsNullOrEmpty(sp));
                foreach (string property in properties)
                {
                    var sortPropertyInfo = property.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (sortPropertyInfo.Length > 2)
                        throw new ClientException("Invalid 'sort' parameter format (" + sort + ").");

                    result.Add(new OrderByProperty
                    {
                        Property = sortPropertyInfo[0],
                        Descending = sortPropertyInfo.Count() >= 2 && sortPropertyInfo[1].ToLower().Equals("desc")
                    });
                }
            }

            return result.ToArray();
        }

        private FilterCriteria[] ParseFilterParameters(string filter, string fparam, string genericfilter, string filters, IDictionary<string, Type[]> filterTypesByName)
        {
            var parsedFilters = new List<FilterCriteria>();

            ParseGenericFilters(filters, parsedFilters);
            ParseGenericFilters(genericfilter, parsedFilters);

            if (!string.IsNullOrEmpty(filter))
            {
                Type filterType = GetFilterType(filter, filterTypesByName);

                object filterInstance;
                if (!string.IsNullOrEmpty(fparam))
                {
                    filterInstance = JsonConvert.DeserializeObject(fparam, filterType);
                    if (filterInstance == null)
                        throw new LegacyClientException("Invalid filter parameter format for filter '" + filter + "', data: '" + fparam + "'.");
                }
                else
                    filterInstance = Activator.CreateInstance(filterType);

                parsedFilters.Add(new FilterCriteria { Filter = filterType.AssemblyQualifiedName, Value = filterInstance });
            }

            foreach (var filterCriteria in parsedFilters)
                if (!string.IsNullOrEmpty(filterCriteria.Filter) && filterCriteria.Value is JToken)
                {
                    Type filterType = GetFilterType(filterCriteria.Filter, filterTypesByName);
                    filterCriteria.Filter = filterType.AssemblyQualifiedName;
                    filterCriteria.Value = ((JToken)filterCriteria.Value).ToObject(filterType);
                }

            return parsedFilters.ToArray();
        }

        private static void ParseGenericFilters(string filters, List<FilterCriteria> parsedFilters)
        {
            if (!string.IsNullOrEmpty(filters))
            {
                var parsedGenericFilter = JsonConvert.DeserializeObject<FilterCriteria[]>(filters);
                if (parsedGenericFilter == null)
                    throw new LegacyClientException("Invalid format of the generic filter: '" + filters + "'.");

                foreach (var genericFilter in parsedGenericFilter)
                    DetectJsonListType(genericFilter);

                parsedFilters.AddRange(parsedGenericFilter);
            }
        }

        private static void DetectJsonListType(FilterCriteria genericFilter)
        {
            if (genericFilter.Value is JArray)
            {
                var jArray = (JArray)genericFilter.Value;
                if (jArray.Count > 0)
                {
                    var elementType = jArray.First().Type;
                    if (jArray.All(item => item.Type == elementType))
                    {
                        switch (elementType)
                        {
                            case JTokenType.String:
                                genericFilter.Value = jArray.ToObject<string[]>();
                                break;
                            case JTokenType.Integer:
                                genericFilter.Value = jArray.ToObject<int[]>();
                                break;
                            case JTokenType.Guid:
                                genericFilter.Value = jArray.ToObject<Guid[]>();
                                break;
                            case JTokenType.Boolean:
                                genericFilter.Value = jArray.ToObject<bool[]>();
                                break;
                            case JTokenType.Date:
                                genericFilter.Value = jArray.ToObject<DateTime[]>();
                                break;
                            case JTokenType.Float:
                                genericFilter.Value = jArray.ToObject<decimal[]>();
                                break;
                        }
                    }
                }
            }
        }

        private Type GetFilterType(string filterName, IDictionary<string, Type[]> filterTypesByName)
        {
            Type filterType = null;

            Type[] matchingTypes = null;
            filterTypesByName.TryGetValue(filterName, out matchingTypes);
            if (matchingTypes != null && matchingTypes.Count() > 1)
                throw new LegacyClientException("Filter type '" + filterName + "' is ambiguous (" + matchingTypes[0].FullName + ", " + matchingTypes[1].FullName + ").");
            if (matchingTypes != null && matchingTypes.Count() == 1)
                filterType = matchingTypes[0];

            if (filterType == null)
                filterType = _domainObjectModel.GetType(filterName);

            if (filterType == null)
                filterType = Type.GetType(filterName);

            if (filterType == null)
                throw new LegacyClientException("Filter type '" + filterName + "' is not available for this data structure.");

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

        public DownloadReportResult DownloadReport<T>(string parameter, string convertFormat)
        {
            object parameterInstance;
            if (!string.IsNullOrEmpty(parameter))
            {
                parameterInstance = JsonConvert.DeserializeObject(parameter, typeof(T));
                if (parameterInstance == null)
                    throw new LegacyClientException("Invalid parameter format for report '" + typeof(T).FullName + "', data: '" + parameter + "'.");
            }
            else
                parameterInstance = Activator.CreateInstance(typeof(T));

            var commandInfo = new DownloadReportCommandInfo
            {
                Report = parameterInstance,
                ConvertFormat = convertFormat
            };

            ProcessingResult result;
            try
            {
                result = _processingEngine.Execute(new[] { commandInfo });
            }
            catch (Autofac.Core.Registration.ComponentNotRegisteredException ex)
            {
                if (ex.Message.Contains(typeof(IReportRepository).FullName))
                    throw new LegacyClientException("Report " + typeof(T).FullName + " does not provide file downloading.", ex);
                else
                    throw;
            }

            CheckForErrors(result);

            var reportCommandResult = (DownloadReportCommandResult)result.CommandResults.Single().Data;

            return new DownloadReportResult
                {
                    ReportFile = reportCommandResult.ReportFile,
                    SuggestedFileName = reportCommandResult.SuggestedFileName
                };
        }
    }
}
