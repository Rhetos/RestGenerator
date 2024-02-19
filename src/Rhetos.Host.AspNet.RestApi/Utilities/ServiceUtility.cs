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

using Newtonsoft.Json;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using Rhetos.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhetos.Host.AspNet.RestApi.Utilities
{
    public class ServiceUtility
    {
        private readonly IProcessingEngine _processingEngine;
        private readonly QueryParameters _queryParameters;

        public ServiceUtility(IRhetosComponent<IProcessingEngine> rhetosProcessingEngine, QueryParameters queryParameters)
        {
            _processingEngine = rhetosProcessingEngine.Value;
            _queryParameters = queryParameters;
        }

        public RecordsAndTotalCountResult<T> GetData<T>(string filter, string fparam, string genericfilter, string filters, int top, int skip, int page, int psize, string sort, bool readRecords, bool readTotalCount)
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
                Filters = _queryParameters.ParseFilterParameters(filter, fparam, genericfilter, filters, typeof(T).FullName),
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

        public T GetDataById<T>(Guid id)
        {
            var filterInstance = new[] { id };

            return (T)ExecuteReadCommand(new ReadCommandInfo
            {
                DataSource = typeof(T).FullName,
                Filters = new[] { new FilterCriteria(typeof(IEnumerable<Guid>), filterInstance) },
                ReadRecords = true
            })
                .Records.FirstOrDefault();
        }

        private ReadCommandResult ExecuteReadCommand(ReadCommandInfo commandInfo)
        {
            return _processingEngine.Execute(commandInfo);
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
            _processingEngine.Execute(commandInfo);
        }

        public void InsertData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToInsert = new[] { (IEntity)entity }
            };

            _processingEngine.Execute(commandInfo);
        }

        public void UpdateData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToUpdate = new[] { (IEntity)entity }
            };

            _processingEngine.Execute(commandInfo);
        }

        public void DeleteData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToDelete = new[] { (IEntity)entity }
            };

            _processingEngine.Execute(commandInfo);
        }

        public DownloadReportResult DownloadReport(Type reportType, string parameterJson, string convertFormat)
        {
            object parameterInstance;
            if (!string.IsNullOrEmpty(parameterJson))
            {
                parameterInstance = JsonConvert.DeserializeObject(parameterJson, reportType);
                if (parameterInstance == null)
                    throw new ClientException($"Invalid parameter format for report '{reportType.FullName}', data: '{parameterJson}'.");
            }
            else
                parameterInstance = Activator.CreateInstance(reportType);

            var commandInfo = new DownloadReportCommandInfo
            {
                Report = parameterInstance,
                ConvertFormat = convertFormat
            };

            DownloadReportCommandResult result;
            try
            {
                result = _processingEngine.Execute(commandInfo);
            }
            catch (Autofac.Core.Registration.ComponentNotRegisteredException ex)
            {
                if (ex.Message.Contains(typeof(IReportRepository).FullName))
                    throw new ClientException($"Report {reportType.FullName} does not provide file downloading.", ex);
                else
                    throw;
            }

            return new DownloadReportResult
                {
                    ReportFile = result.ReportFile,
                    SuggestedFileName = result.SuggestedFileName
                };
        }
    }
}
