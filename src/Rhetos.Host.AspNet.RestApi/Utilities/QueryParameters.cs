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

using Rhetos.Dom;
using Rhetos.Dom.DefaultConcepts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhetos.Host.AspNet.RestApi.Utilities
{
    public class QueryParameters
    {
        private readonly IDomainObjectModel domainObjectModel;

        public QueryParameters(IRhetosComponent<IDomainObjectModel> rhetosDomainObjectModel)
        {
            this.domainObjectModel = rhetosDomainObjectModel.Value;
        }

        /// <param name="filter">Legacy</param>
        /// <param name="fparam">Legacy</param>
        /// <param name="genericfilter">Legacy</param>
        public FilterCriteria[] ParseFilterParameters(string filter, string fparam, string genericfilter, string filters, Tuple<string, Type>[] filterTypes)
        {
            var parsedFilters = new List<FilterCriteria>();

            if (!string.IsNullOrEmpty(filters))
                parsedFilters.AddRange(Json.DeserializeOrException<FilterCriteria[]>(filters));

            // Legacy:
            if (!string.IsNullOrEmpty(genericfilter))
                parsedFilters.AddRange(Json.DeserializeOrException<FilterCriteria[]>(genericfilter));

            // Legacy:
            if (!string.IsNullOrEmpty(filter))
            {
                Type filterType = GetFilterType(filter, filterTypes);
                parsedFilters.Add(new FilterCriteria
                {
                    Filter = filterType.AssemblyQualifiedName,
                    Value = !string.IsNullOrEmpty(fparam)
                        ? Json.DeserializeOrException(fparam, filterType)
                        : Activator.CreateInstance(filterType)
                });
            }

            foreach (var filterCriteria in parsedFilters)
            {
                if (!string.IsNullOrEmpty(filterCriteria.Filter))
                {
                    // Specify the exact filter type from a simplified filter name.
                    Type filterType = GetFilterType(filterCriteria.Filter, filterTypes);
                    filterCriteria.Filter = filterType.AssemblyQualifiedName;

                    // Resolve partially deserialized filter parameter with known filter types, if a parameter value is not completely deserialized.
                    filterCriteria.Value = Json.FinishPartiallyDeserializedObject(filterCriteria.Value, filterType);
                }

                // Resolve partially deserialized arrays, if a parameter value is not completely deserialized.
                // This heuristics is important only for generic *property* filter (filterCriteria.Filter == null),
                // otherwise it will be resolved previously with FinishPartiallyDeserializedObject.
                filterCriteria.Value = Json.FinishPartiallyDeserializedArray(filterCriteria.Value);
            }

            return parsedFilters.ToArray();
        }

        private Type GetFilterType(string filterName, Tuple<string, Type>[] filterTypes)
        {
            Type filterType = null;

            List<Type> matchingTypes = filterTypes
                .Where(f => f.Item1.Equals(filterName)).Select(f => f.Item2).Distinct().ToList();

            if (matchingTypes.Count > 1)
                throw new ClientException($"Filter type '{filterName}' is ambiguous ({matchingTypes.First().FullName}, {matchingTypes.Last().FullName})." +
                    $" Please specify full filter name.");

            if (matchingTypes.Count == 1)
                filterType = matchingTypes.Single();

            // TODO: Remove usage of IDomainObjectModel.GetType and Type.GetType, to make REST API more predictable, since run-time type resolution depends on currently loaded assemblies,
            // and may behave unpredictable on startup. The filterTypes should contain all available filters.
            // This changes should be configurable to allow backward compatibility for old applications.

            if (filterType == null)
                filterType = domainObjectModel.GetType(filterName);

            if (filterType == null)
                filterType = Type.GetType(filterName);

            if (filterType == null)
                throw new ClientException($"Filter type '{filterName}' is not available for this data structure. Please make sure that correct filter name is provided in web request.");

            return filterType;
        }
    }
}
