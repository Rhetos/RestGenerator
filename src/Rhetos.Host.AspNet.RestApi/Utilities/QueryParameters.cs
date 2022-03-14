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
using System;
using System.Collections.Generic;

namespace Rhetos.Host.AspNet.RestApi.Utilities
{
    public class QueryParameters
    {
        private readonly GenericFilterHelper _genericFilterHelper;

        public QueryParameters(IRhetosComponent<GenericFilterHelper> genericFilterHelper)
        {
            _genericFilterHelper = genericFilterHelper.Value;
        }

        /// <param name="filter">Legacy</param>
        /// <param name="fparam">Legacy</param>
        /// <param name="genericfilter">Legacy</param>
        public FilterCriteria[] ParseFilterParameters(string filter, string fparam, string genericfilter, string filters, string dataStructureFullName)
        {
            var parsedFilters = new List<FilterCriteria>();

            if (!string.IsNullOrEmpty(filters))
                parsedFilters.AddRange(JsonHelper.DeserializeOrException<FilterCriteria[]>(filters));

            // Legacy:
            if (!string.IsNullOrEmpty(genericfilter))
                parsedFilters.AddRange(JsonHelper.DeserializeOrException<FilterCriteria[]>(genericfilter));

            // Legacy:
            if (!string.IsNullOrEmpty(filter))
            {
                Type filterType = _genericFilterHelper.GetFilterType(dataStructureFullName, filter);
                parsedFilters.Add(new FilterCriteria
                {
                    Filter = filter,
                    Value = !string.IsNullOrEmpty(fparam)
                        ? JsonHelper.DeserializeOrException(fparam, filterType)
                        : Activator.CreateInstance(filterType)
                });
            }

            foreach (var filterCriteria in parsedFilters)
            {
                if (!string.IsNullOrEmpty(filterCriteria.Filter))
                {
                    // Verifying the filter name format:
                    Type filterType = _genericFilterHelper.GetFilterType(dataStructureFullName, filterCriteria.Filter);

                    // Resolve partially deserialized filter parameter with known filter types, if a parameter value is not completely deserialized.
                    filterCriteria.Value = JsonHelper.FinishPartiallyDeserializedObject(filterCriteria.Value, filterType);
                }

                // Resolve partially deserialized arrays, if a parameter value is not completely deserialized.
                // This heuristics is important only for generic *property* filter (filterCriteria.Filter == null),
                // otherwise it will be resolved previously with FinishPartiallyDeserializedObject.
                filterCriteria.Value = JsonHelper.FinishPartiallyDeserializedArray(filterCriteria.Value);
            }

            return parsedFilters.ToArray();
        }
    }
}
