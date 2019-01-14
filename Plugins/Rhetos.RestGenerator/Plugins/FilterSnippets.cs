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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Rhetos.Compiler;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;
using Rhetos.RestGenerator;
using Rhetos.Utilities;

namespace Rhetos.RestGenerator.Plugins
{
    public class FilterSnippets
    {
        public FilterSnippets()
        {
        }

        public string ExpectedFilterTypesSnippet(DataStructureInfo source, string filterParameter)
        {
            var fullParameterName = filterParameter;
            if (System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(fullParameterName))
                fullParameterName = source.Module.Name + "." + fullParameterName;

            // Performance improvement for filter name evaluation, to avoid using Type.GetType() at runtime. It might be removed.
            string snippet = $"Tuple.Create(\"{fullParameterName}\", typeof({fullParameterName})),\r\n                ";

            // This helper allows client to skip the default namespace names:
            var shortName = TryExtractShortName(fullParameterName, source);
            if (shortName != null)
                snippet += $"Tuple.Create(\"{shortName}\", typeof({fullParameterName})),\r\n                ";

            return snippet;
        }

        private static readonly string[] _defaultNamespaces = new string[]
        {
            "Common.",
            "System.",
            "System.Collections.Generic.",
            "Rhetos.Dom.DefaultConcepts.",
        };

        private string TryExtractShortName(string typeName, DataStructureInfo source)
        {
            var removablePrefixes = _defaultNamespaces
                .Concat(new[] { source.Module.Name + "." })
                .OrderByDescending(p => p.Length)
                .ToList();
            var removablePrefix = removablePrefixes.FirstOrDefault(prefix => typeName.StartsWith(prefix));
            if (removablePrefix != null)
                return typeName.Substring(removablePrefix.Length);
            return null;
        }
    }
}
