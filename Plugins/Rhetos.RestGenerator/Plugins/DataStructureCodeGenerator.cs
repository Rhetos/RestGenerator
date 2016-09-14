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

        public static readonly CsTag<DataStructureInfo> AdditionalPropertyInitialization = "AdditionalPropertyInitialization";
        public static readonly CsTag<DataStructureInfo> AdditionalPropertyConstructorParameter = "AdditionalPropertyConstructorParameter";
        public static readonly CsTag<DataStructureInfo> AdditionalPropertyConstructorSetProperties = "AdditionalPropertyConstructorSetProperties";

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
        {2}

        public RestService{0}{1}(ServiceUtility serviceUtility{3})
        {{
            _serviceUtility = serviceUtility;
            {4}
        }}
    
        public static readonly IDictionary<string, Type[]> FilterTypes = new List<Tuple<string, Type>>
            {{
                " + FilterTypesTag.Evaluate(info) + @"
            }}
            .GroupBy(typeName => typeName.Item1)
            .ToDictionary(g => g.Key, g => g.Select(typeName => typeName.Item2).Distinct().ToArray());

        // [Obsolete] parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json)]
        public void Get(string filter, string fparam, string genericfilter, string filters, string sort)
        {{
            Console.WriteLine("""" );
            var data = _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, filters, FilterTypes, 0, 0, 0, 0, sort,
                readRecords: true, readTotalCount: false);
            var responseBody = Newtonsoft.Json.JsonConvert.SerializeObject(new RecordsResult<{0}.{1}> {{ Records = data.Records }});
            HttpContext.Current.Response.ContentType = ""application/json; charset=utf-8"";
            HttpContext.Current.Response.Write(responseBody);
        }}

        [Obsolete]
        [OperationContract]
        [WebGet(UriTemplate = ""/Count?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json)]
        public void GetCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {{
            var data = _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, filters, FilterTypes, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            var responseBody = Newtonsoft.Json.JsonConvert.SerializeObject(new CountResult {{ TotalRecords = data.TotalCount }});
            HttpContext.Current.Response.ContentType = ""application/json; charset=utf-8"";
            HttpContext.Current.Response.Write(responseBody);
        }}

        // [Obsolete] parameters: filter, fparam, genericfilter (use filters).
        [OperationContract]
        [WebGet(UriTemplate = ""/TotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json)]
        public void GetTotalCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {{
            var data = _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, filters, FilterTypes, 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            var responseBody = Newtonsoft.Json.JsonConvert.SerializeObject(new TotalCountResult {{ TotalCount = data.TotalCount }});
            HttpContext.Current.Response.ContentType = ""application/json; charset=utf-8"";
            HttpContext.Current.Response.Write(responseBody);
        }}

        // [Obsolete] parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/RecordsAndTotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&top={{top}}&skip={{skip}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json)]
        public void GetRecordsAndTotalCount(string filter, string fparam, string genericfilter, string filters, int top, int skip, int page, int psize, string sort)
        {{
            var responseBody = Newtonsoft.Json.JsonConvert.SerializeObject( _serviceUtility.GetData<{0}.{1}>(filter, fparam, genericfilter, 
                    filters, FilterTypes, top, skip, page, psize, sort, readRecords: true, readTotalCount: true));
            HttpContext.Current.Response.ContentType = ""application/json; charset=utf-8"";
            HttpContext.Current.Response.Write(responseBody);
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/{{id}}"", RequestFormat = WebMessageFormat.Json)]
        public void GetById(string id)
        {{
            var result = _serviceUtility.GetDataById<{0}.{1}>(id);
            if (result == null)
                throw new Rhetos.LegacyClientException(""There is no resource of this type with a given ID."") {{ HttpStatusCode = HttpStatusCode.NotFound }};
            var responseBody = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            HttpContext.Current.Response.ContentType = ""application/json; charset=utf-8"";
            HttpContext.Current.Response.Write(responseBody);
        }}

        " + AdditionalOperationsTag.Evaluate(info) + @"
    }}
    ",
            info.Module.Name,
            info.Name,
            AdditionalPropertyInitialization.Evaluate(info),
            AdditionalPropertyConstructorParameter.Evaluate(info),
            AdditionalPropertyConstructorSetProperties.Evaluate(info)
            );
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