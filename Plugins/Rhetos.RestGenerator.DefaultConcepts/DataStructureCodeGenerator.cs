/*
    Copyright (C) 2013 Omega software d.o.o.

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

namespace Rhetos.RestGenerator.DefaultConcepts
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
        private ServiceLoader _serviceLoader;

        public RestService{0}{1}(ServiceLoader serviceLoader) 
        {{
            _serviceLoader = serviceLoader;
        }}
    
        private static readonly IDictionary<string, Type[]> {0}{1}FilterTypes = new List<Tuple<string, Type>>
            {{
                " + FilterTypesTag.Evaluate(info) + @"
            }}
            .GroupBy(typeName => typeName.Item1)
            .ToDictionary(g => g.Key, g => g.Select(typeName => typeName.Item2).Distinct().ToArray());

        [OperationContract]
        [WebGet(UriTemplate = ""/?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public GetResult<{0}.{1}> Get(string filter, string fparam, string genericfilter, int page, int psize, string sort)
        {{
            object filterObject;
            Rhetos.Dom.DefaultConcepts.FilterCriteria[] genericFilter;
            ServiceLoader.GetFilterParameters(filter, fparam, genericfilter, null, out genericFilter, out filterObject);
            var queryResult = _serviceLoader.GetData<{0}.{1}>(filterObject, genericFilter, page, psize, sort);
            return new GetResult<{0}.{1}> {{ Records = queryResult.Records }};
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/Count?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public CountResult Count(string filter, string fparam, string genericfilter, int page, int psize, string sort)
        {{
            object filterObject;
            Rhetos.Dom.DefaultConcepts.FilterCriteria[] genericFilter;
            ServiceLoader.GetFilterParameters(filter, fparam, genericfilter, null, out genericFilter, out filterObject);
            var queryResult = _serviceLoader.GetData<{0}.{1}>(filterObject, genericFilter, page, psize, sort);
            return new CountResult {{ TotalRecords = queryResult.TotalRecords }};
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/{{id}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public {0}.{1} GetById(string id)
        {{
            var filter = new [] {{ Guid.Parse(id) }};

            var result = _serviceLoader.GetData<{0}.{1}>(filter).Records.FirstOrDefault();
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

        private static bool _isInitialCallMade;

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
                GenerateInitialCode(codeBuilder);
                codeBuilder.AddReferencesFromDependency(typeof(System.Runtime.Serialization.Json.DataContractJsonSerializer));

                codeBuilder.InsertCode(ServiceRegistrationCodeSnippet(info), InitialCodeGenerator.ServiceRegistrationTag);
                codeBuilder.InsertCode(ServiceInitializationCodeSnippet(info), InitialCodeGenerator.ServiceInitializationTag);
                codeBuilder.InsertCode(ServiceDefinitionCodeSnippet(info), InitialCodeGenerator.RhetosRestClassesTag);

                if (info is IWritableOrmDataStructure) 
                    WritableOrmDataStructureCodeGenerator.GenerateCode(conceptInfo, codeBuilder);
            }
        }

        private static void GenerateInitialCode(ICodeBuilder codeBuilder)
        {
            if (_isInitialCallMade)
                return;
            _isInitialCallMade = true;

            codeBuilder.InsertCode(@"

        public QueryResult<T> GetData<T>(object filter, Rhetos.Dom.DefaultConcepts.FilterCriteria[] genericFilter=null, int page=0, int psize=0, string sort="""")
        {
            _performanceLogger.Write(_stopwatch, ""RestService: GetData(""+typeof (T).FullName+"") Started."");
            _commandsLogger.Trace(""GetData("" + typeof (T).FullName + "");"");
            var commandInfo = new QueryDataSourceCommandInfo
                                  {
                                      DataSource = typeof (T).FullName,
                                      Filter = filter,
                                      GenericFilter = genericFilter,
                                      PageNumber = page,
                                      RecordsPerPage = psize
                                  };

            if (!String.IsNullOrWhiteSpace(sort))
            {
                var sortParameters = sort.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                commandInfo.OrderByProperty = sortParameters[0];
                commandInfo.OrderDescending = sortParameters.Count() >= 2 && sortParameters[1].ToLower().Equals(""desc"");
            }

            var result = _processingEngine.Execute(new[]{commandInfo});
            CheckForErrors(result);

            var resultData = (QueryDataSourceCommandResult)(((Rhetos.XmlSerialization.XmlBasicData<QueryDataSourceCommandResult>)(result.CommandResults.Single().Data)).Value);

            commandInfo.Filter = null;
            commandInfo.GenericFilter = null;

            _performanceLogger.Write(_stopwatch, ""RestService: GetData(""+typeof (T).FullName+"") Executed."");
            return new QueryResult<T>
            {
                Records = resultData.Records.Select(o => (T)o).ToList(),
                TotalRecords = resultData.TotalRecords,
                CommandArguments = commandInfo
            };
        }

        private static T DeserializeJson<T>(string json)
        {
            return (T) DeserializeJson(typeof(T), json);
        }

        private static object DeserializeJson(Type type, string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(type);
                return serializer.ReadObject(stream);
            }
        }

        public static void GetFilterParameters(string filter, string fparam, string genericfilter, IDictionary<string, Type[]> FilterTypes, out Rhetos.Dom.DefaultConcepts.FilterCriteria[] genericFilterInstance, out object filterInstance) 
        {
            genericFilterInstance = null;
            if (!string.IsNullOrEmpty(genericfilter))
            {
                genericFilterInstance = DeserializeJson<Rhetos.Dom.DefaultConcepts.FilterCriteria[]>(genericfilter);
                if (genericFilterInstance == null)
                        throw new Rhetos.UserException(""Invalid format of the generic filter: '"" + genericfilter + ""'."");
            }
            Type filterType = null;
            if (!string.IsNullOrEmpty(filter))
            {
                Type[] filterTypes = null;
                FilterTypes.TryGetValue(filter, out filterTypes);
                if (filterTypes != null && filterTypes.Count() > 1)
                    throw new Rhetos.UserException(""Filter type '"" + filter + ""' is ambiguous ("" + filterTypes[0].FullName + "", "" + filterTypes[1].FullName +"")."");
                if (filterTypes != null && filterTypes.Count() == 1)
                    filterType = filterTypes[0];

                if (filterType == null)    
                    filterType = Type.GetType(filter);

                if (filterType == null && Rhetos.Utilities.XmlUtility.Dom != null)
                    filterType = Rhetos.Utilities.XmlUtility.Dom.GetType(filter);

                if (filterType == null)
                    throw new Rhetos.UserException(""Filter type '"" + filter + ""' is not recognised."");
            }
            
            filterInstance = null;
            if (filterType != null)
            {
                if (!string.IsNullOrEmpty(fparam))
                {
                    filterInstance = DeserializeJson(filterType, fparam);
                    if (filterInstance == null)
                        throw new Rhetos.UserException(""Invalid filter parameter format for filter '"" + filter + ""', data: '"" + fparam + ""'."");
                }
                else
                    filterInstance = Activator.CreateInstance(filterType);
            }
        }

", InitialCodeGenerator.ServiceLoaderMembersTag);

            codeBuilder.InsertCode(@"
    public class GetResult<T>
    {
        public IList<T> Records { get; set; }
    }

    public class CountResult
    {
        public int TotalRecords { get; set; }
    }

    public class QueryResult<T>
    {
        public IList<T> Records { get; set; }
        public int TotalRecords { get; set; }
        public QueryDataSourceCommandInfo CommandArguments { get; set; }
    }

", InitialCodeGenerator.RhetosRestClassesTag);

        }
    }
}