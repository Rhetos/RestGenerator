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
using System.ComponentModel.Composition;
using System.Globalization;
using System.Xml;
using Rhetos.Compiler;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;
using Rhetos.RestGenerator;
using Rhetos.Utilities;

namespace Rhetos.RestGenerator.Plugins
{
    [Export(typeof(IRestGeneratorPlugin))]
    [ExportMetadata(MefProvider.Implements, typeof(DataStructureInfo))]
    public class DataStructureCodeGenerator : IRestGeneratorPlugin
    {
        public static readonly CsTag<DataStructureInfo> FilterTypesTag = "FilterTypes";

        public static readonly CsTag<DataStructureInfo> AdditionalOperationsTag = "AdditionalOperations";

        private static string ServiceRegistrationCodeSnippet(DataStructureInfo info)
        {
            return string.Format(@"builder.RegisterType<RestService{0}{1}>().InstancePerLifetimeScope();
            ", info.Module.Name, info.Name);
        }

        private static string ServiceInitializationCodeSnippet(DataStructureInfo info)
        {
            return string.Format(@"System.Web.Routing.RouteTable.Routes.Add(new System.ServiceModel.Activation.ServiceRoute(""Rest/{0}/{1}"", 
                new RestServiceHostFactory(), typeof(RestService{0}{1})));
            ", info.Module.Name, info.Name);
        }

        private static string ServiceDefinitionCodeSnippet(DataStructureInfo info)
        {
            return string.Format(@"
    [System.ServiceModel.ServiceContract]
    [System.ServiceModel.Activation.AspNetCompatibilityRequirements(RequirementsMode = System.ServiceModel.Activation.AspNetCompatibilityRequirementsMode.Allowed)]
    public class RestService{0}{1}
    {{
        private ServiceUtility _serviceUtility;

        public RestService{0}{1}(ServiceUtility serviceUtility)
        {{
            _serviceUtility = serviceUtility;
        }}
    
        private static readonly IDictionary<string, Type[]> {0}{1}FilterTypes = new List<Tuple<string, Type>>
            {{
                " + FilterTypesTag.Evaluate(info) + @"
            }}
            .GroupBy(typeName => typeName.Item1)
            .ToDictionary(g => g.Key, g => g.Select(typeName => typeName.Item2).Distinct().ToArray());

        // [Obsolete] parameters: filter, fparam, page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&top={{top}}&skip={{skip}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public RecordsResult<{0}.{1}> Get(string filter, string fparam, string genericfilter, int top, int skip, int page, int psize, string sort)
        {{
            var data = _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, {0}{1}FilterTypes, top, skip, page, psize, sort,
                readRecords: true, readTotalCount: false);
            return new RecordsResult<{0}.{1}> {{ Records = data.Records }};
        }}

        [Obsolete]
        [OperationContract]
        [WebGet(UriTemplate = ""/Count?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public CountResult GetCount(string filter, string fparam, string genericfilter, string sort)
        {{
            var data = _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, {0}{1}FilterTypes, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new CountResult {{ TotalRecords = data.TotalCount }};
        }}

        // [Obsolete] parameters: filter, fparam.
        [OperationContract]
        [WebGet(UriTemplate = ""/TotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public TotalCountResult GetTotalCount(string filter, string fparam, string genericfilter, string sort)
        {{
            var data = _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, {0}{1}FilterTypes, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new TotalCountResult {{ TotalCount = data.TotalCount }};
        }}

        // [Obsolete] parameters: filter, fparam, page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/RecordsAndTotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&top={{top}}&skip={{skip}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public RecordsAndTotalCountResult<{0}.{1}> GetRecordsAndTotalCount(string filter, string fparam, string genericfilter, int top, int skip, int page, int psize, string sort)
        {{
            return _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, {0}{1}FilterTypes, top, skip, page, psize, sort,
                readRecords: true, readTotalCount: true);
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/{{id}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public {0}.{1} GetById(string id)
        {{
            var result = _serviceUtility.GetDataById<{0}.{1}>(id);
            if (result == null)
                throw new WebFaultException<string>(""There is no resource of this type with a given ID."", HttpStatusCode.NotFound);
            return result;
        }}

        " + AdditionalOperationsTag.Evaluate(info) + @"
    }}

    ",
            info.Module.Name,
            info.Name);
        }

        public static bool IsTypeSupported(DataStructureInfo conceptInfo)
        {
            return conceptInfo is EntityInfo
                || conceptInfo is BrowseDataStructureInfo
                || conceptInfo is LegacyEntityInfo
                || conceptInfo is LegacyEntityWithAutoCreatedViewInfo
                || conceptInfo is SqlQueryableInfo
                || conceptInfo is QueryableExtensionInfo
                || conceptInfo is ComputedInfo;
        }

        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            DataStructureInfo info = (DataStructureInfo)conceptInfo;

            if (IsTypeSupported(info))
            {
                codeBuilder.InsertCode(ServiceRegistrationCodeSnippet(info), InitialCodeGenerator.ServiceRegistrationTag);
                codeBuilder.InsertCode(ServiceInitializationCodeSnippet(info), InitialCodeGenerator.ServiceInitializationTag);
                codeBuilder.InsertCode(ServiceDefinitionCodeSnippet(info), InitialCodeGenerator.RhetosRestClassesTag);
                codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Processing.DefaultCommands.ReadCommandResult));
                codeBuilder.AddReferencesFromDependency(typeof(Newtonsoft.Json.Linq.JToken));

                if (info is IWritableOrmDataStructure) 
                    WritableOrmDataStructureCodeGenerator.GenerateCode(conceptInfo, codeBuilder);
            }
        }
    }
}