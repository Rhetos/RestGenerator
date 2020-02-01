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
    [ExportMetadata(MefProvider.Implements, typeof(DataStructureInfo))]
    public class DataStructureCodeGenerator : IRestGeneratorPlugin
    {
        public static readonly CsTag<DataStructureInfo> FilterTypesTag = "FilterTypes";

        public static readonly CsTag<DataStructureInfo> AdditionalOperationsTag = "AdditionalOperations";

        public static readonly CsTag<DataStructureInfo> AdditionalPropertyInitialization = "AdditionalPropertyInitialization";
        public static readonly CsTag<DataStructureInfo> AdditionalPropertyConstructorParameter = "AdditionalPropertyConstructorParameter";
        public static readonly CsTag<DataStructureInfo> AdditionalPropertyConstructorSetProperties = "AdditionalPropertyConstructorSetProperties";

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
        {AdditionalPropertyInitialization.Evaluate(info)}

        public RestService{info.Module.Name}{info.Name}(ServiceUtility serviceUtility{AdditionalPropertyConstructorParameter.Evaluate(info)})
        {{
            _serviceUtility = serviceUtility;
            {AdditionalPropertyConstructorSetProperties.Evaluate(info)}
        }}
    
        public static readonly Tuple<string, Type>[] FilterTypes = new Tuple<string, Type>[]
            {{
                {FilterTypesTag.Evaluate(info)}
            }};

        // [Obsolete] parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&top={{top}}&skip={{skip}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public RecordsResult<{info.Module.Name}.{info.Name}> Get(string filter, string fparam, string genericfilter, string filters, int top, int skip, int page, int psize, string sort)
        {{
            var data = _serviceUtility.GetData<{info.Module.Name}.{info.Name}>(filter, fparam, genericfilter, filters, FilterTypes, top, skip, page, psize, sort,
                readRecords: true, readTotalCount: false);
            return new RecordsResult<{info.Module.Name}.{info.Name}> {{ Records = data.Records }};
        }}

        [Obsolete]
        [OperationContract]
        [WebGet(UriTemplate = ""/Count?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public CountResult GetCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {{
            var data = _serviceUtility.GetData<{info.Module.Name}.{info.Name}>(filter, fparam, genericfilter, filters, FilterTypes, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new CountResult {{ TotalRecords = data.TotalCount }};
        }}

        // [Obsolete] parameters: filter, fparam, genericfilter (use filters).
        [OperationContract]
        [WebGet(UriTemplate = ""/TotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public TotalCountResult GetTotalCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {{
            var data = _serviceUtility.GetData<{info.Module.Name}.{info.Name}>(filter, fparam, genericfilter, filters, FilterTypes, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new TotalCountResult {{ TotalCount = data.TotalCount }};
        }}

        // [Obsolete] parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/RecordsAndTotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&top={{top}}&skip={{skip}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public RecordsAndTotalCountResult<{info.Module.Name}.{info.Name}> GetRecordsAndTotalCount(string filter, string fparam, string genericfilter, string filters, int top, int skip, int page, int psize, string sort)
        {{
            return _serviceUtility.GetData<{info.Module.Name}.{info.Name}>(filter, fparam, genericfilter, filters, FilterTypes, top, skip, page, psize, sort,
                readRecords: true, readTotalCount: true);
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/{{id}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public {info.Module.Name}.{info.Name} GetById(string id)
        {{
            var result = _serviceUtility.GetDataById<{info.Module.Name}.{info.Name}>(id);
            if (result == null)
                throw new Rhetos.LegacyClientException(""There is no resource of this type with a given ID."") {{ HttpStatusCode = HttpStatusCode.NotFound, Severe = false }};
            return result;
        }}

        {AdditionalOperationsTag.Evaluate(info)}
    }}
    ";
        }

        public static bool IsTypeSupported(DataStructureInfo conceptInfo)
        {
            return conceptInfo is IOrmDataStructure
                || conceptInfo is BrowseDataStructureInfo
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