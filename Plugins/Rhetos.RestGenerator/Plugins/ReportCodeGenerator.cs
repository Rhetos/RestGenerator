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

using Rhetos.Compiler;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;
using System.ComponentModel.Composition;

namespace Rhetos.RestGenerator.Plugins
{
    [Export(typeof(IRestGeneratorPlugin))]
    [ExportMetadata(MefProvider.Implements, typeof(ReportDataInfo))]
    public class ReportCodeGenerator : IRestGeneratorPlugin
    {
        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            var info = (ReportDataInfo)conceptInfo;

            codeBuilder.InsertCode(ServiceRegistrationCodeSnippet(info), InitialCodeGenerator.ServiceRegistrationTag);
            codeBuilder.InsertCode(ServiceInitializationCodeSnippet(info), InitialCodeGenerator.ServiceInitializationTag);
            codeBuilder.InsertCode(ServiceDefinitionCodeSnippet(info), InitialCodeGenerator.RhetosRestClassesTag);

            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.RestGenerator.Utilities.ServiceUtility));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.RestGenerator.Utilities.DownloadReportResult));
            codeBuilder.AddReferencesFromDependency(typeof(Newtonsoft.Json.JsonConvert));
        }

        public static readonly CsTag<DataStructureInfo> FilterTypesTag = "FilterTypes";

        public static readonly CsTag<DataStructureInfo> AdditionalOperationsTag = "AdditionalOperations";

        private static string ServiceRegistrationCodeSnippet(DataStructureInfo info)
        {
            return $@"builder.RegisterType<RestService{info.Module.Name}{info.Name}>().InstancePerLifetimeScope();
            ";
        }

        private static string ServiceInitializationCodeSnippet(DataStructureInfo info)
        {
            return $@"System.Web.Routing.RouteTable.Routes.Add(new System.ServiceModel.Activation.ServiceRoute(""Rest/{info.Module.Name}/{info.Name}"", 
                new RestServiceHostFactory(), typeof(RestService{info.Module.Name}{info.Name})));
            ";
        }

        private static string ServiceDefinitionCodeSnippet(DataStructureInfo info)
        {
            return $@"
    [System.ServiceModel.ServiceContract]
    [System.ServiceModel.Activation.AspNetCompatibilityRequirements(RequirementsMode = System.ServiceModel.Activation.AspNetCompatibilityRequirementsMode.Allowed)]
    public class RestService{info.Module.Name}{info.Name}
    {{
        private ServiceUtility _serviceUtility;

        public RestService{info.Module.Name}{info.Name}(ServiceUtility serviceUtility)
        {{
            _serviceUtility = serviceUtility;
        }}
    
        [OperationContract]
        [WebGet(UriTemplate = ""/?parameter={{parameter}}&convertFormat={{convertFormat}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public DownloadReportResult DownloadReport(string parameter, string convertFormat)
        {{
            return _serviceUtility.DownloadReport<{info.Module.Name}.{info.Name}>(parameter, convertFormat);
        }}

        {AdditionalOperationsTag.Evaluate(info)}
    }}

    ";
        }
    }
}