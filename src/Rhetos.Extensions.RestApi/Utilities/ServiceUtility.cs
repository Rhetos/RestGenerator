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

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Host.AspNet;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;

namespace Rhetos.Extensions.RestApi.Utilities
{
    public class ServiceUtility
    {
        private readonly IProcessingEngine _processingEngine;
        private readonly QueryParameters _queryParameters;

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

        public ServiceUtility(IRhetosComponent<IProcessingEngine> rhetosProcessingEngine, QueryParameters queryParameters)
        {
            _processingEngine = rhetosProcessingEngine.Value;
            _queryParameters = queryParameters;
        }

        public RecordsAndTotalCountResult<T> GetData<T>(string filter, string fparam, string genericfilter, string filters, Tuple<string, Type>[] filterTypes, int top, int skip, int page, int psize, string sort, bool readRecords, bool readTotalCount)
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
                Filters = _queryParameters.ParseFilterParameters(filter, fparam, genericfilter, filters, filterTypes),
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
                throw new ClientException("Invalid format of GUID parametar 'ID'.");

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
            var result = _processingEngine.Execute(new[] { commandInfo });
            CheckForErrors(result);
            var resultData = (ReadCommandResult)(((Rhetos.XmlSerialization.XmlBasicData<ReadCommandResult>)(result.CommandResults.Single().Data)).Value);

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
                        throw new ClientException($"Invalid 'sort' parameter format ({sort}).");

                    result.Add(new OrderByProperty
                    {
                        Property = sortPropertyInfo[0],
                        Descending = sortPropertyInfo.Count() >= 2 && sortPropertyInfo[1].Equals("desc", StringComparison.InvariantCultureIgnoreCase)
                    });
                }
            }

            return result.ToArray();
        }

        public void Execute<T>(T action)
        {
            if (action == null)
                action = Activator.CreateInstance<T>();
        
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
#pragma warning disable CS0618 // 'LegacyClientException' is obsolete: 'Use ClientException instead.'
                    throw new LegacyClientException($"Invalid parameter format for report '{typeof(T).FullName}', data: '{parameter}'.");
#pragma warning restore CS0618 // 'LegacyClientException' is obsolete: 'Use ClientException instead.'
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
#pragma warning disable CS0618 // 'LegacyClientException' is obsolete: 'Use ClientException instead.'
                    throw new LegacyClientException($"Report {typeof(T).FullName} does not provide file downloading.", ex);
#pragma warning restore CS0618 // 'LegacyClientException' is obsolete: 'Use ClientException instead.'
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
