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

using Rhetos.Compiler;
using Rhetos.Extensibility;
using Rhetos.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using ICodeGenerator = Rhetos.Compiler.ICodeGenerator;

namespace Rhetos.RestGenerator
{
    [Export(typeof(IGenerator))]
    public class RestGenerator : IGenerator
    {
        private readonly IPluginsContainer<IRestGeneratorPlugin> _plugins;
        private readonly ICodeGenerator _codeGenerator;
        private readonly IAssemblyGenerator _assemblyGenerator;
        private readonly ILogger _logger;
        private readonly ILogger _sourceLogger;

        public static string GetAssemblyPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generated", "RestService.dll");
        }

        public RestGenerator(
            IPluginsContainer<IRestGeneratorPlugin> plugins,
            ICodeGenerator codeGenerator,
            ILogProvider logProvider,
            IAssemblyGenerator assemblyGenerator
        )
        {
            _plugins = plugins;
            _codeGenerator = codeGenerator;
            _assemblyGenerator = assemblyGenerator;

            _logger = logProvider.GetLogger("RestGenerator");
            _sourceLogger = logProvider.GetLogger("Rest service");
        }

        public void Generate()
        {
            IAssemblySource assemblySource = _codeGenerator.ExecutePlugins(_plugins, "/*", "*/", new InitialCodeGenerator());
            _logger.Trace("References: " + string.Join(", ", assemblySource.RegisteredReferences));
            _sourceLogger.Trace(assemblySource.GeneratedCode);
            _assemblyGenerator.Generate(assemblySource, GetAssemblyPath());
        }

        public IEnumerable<string> Dependencies
        {
            get { return null; }
        }
    }
}
