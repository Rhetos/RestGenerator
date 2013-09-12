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
using Rhetos.REST2Generator;

namespace Rhetos.REST2Generator.DefaultConcepts
{
    [Export(typeof(IRESTGeneratorPlugin))]
    [ExportMetadata(MefProvider.Implements, typeof(DataStructureInfo))]
    public class DataStructureCodeGenerator : IRESTGeneratorPlugin
    {
        public static readonly CsTag<DataStructureInfo> FilterTypesTag = "FilterTypes";

        private static string ServiceRegistrationCodeSnippet(DataStructureInfo info)
        {
            return string.Format(@"
    [System.ComponentModel.Composition.Export(typeof(Module))]
    public class RESTService{0}{1}ModuleConfiguration : Module
    {{
        protected override void Load(ContainerBuilder builder)
        {{
            builder.RegisterType<RESTService{0}{1}>().InstancePerLifetimeScope();
            base.Load(builder);
        }}
    }}

    [System.ComponentModel.Composition.Export(typeof(Rhetos.IService))]
    public class RESTService{0}{1}Initializer : Rhetos.IService
    {{
        public void Initialize()
        {{
            System.Web.Routing.RouteTable.Routes.Add(new System.ServiceModel.Activation.ServiceRoute(""REST/{0}/{1}"", 
                          new RESTServiceHostFactory(), typeof(Services.RESTService{0}{1})));
        }}
    }}

    ",
            info.Module.Name,
            info.Name);
        }


        private static string ServiceDefinitionCodeSnippet(DataStructureInfo info)
        {
            return string.Format(@"
    [System.ServiceModel.ServiceContract]
    [System.ServiceModel.Activation.AspNetCompatibilityRequirements(RequirementsMode = System.ServiceModel.Activation.AspNetCompatibilityRequirementsMode.Allowed)]
    public class RESTService{0}{1}
    {{
        private ServiceLoader _serviceLoader;

        public RESTService{0}{1}(ServiceLoader serviceLoader) 
        {{
            _serviceLoader = serviceLoader;
        }}
    
        private static readonly IDictionary<string, Type[]> {0}{1}FilterTypes = new List<Tuple<string, Type>>
            {{
                " + FilterTypesTag.Evaluate(info) + @"
            }}
            .GroupBy(typeName => typeName.Item1)
            .ToDictionary(g => g.Key, g => g.Select(typeName => typeName.Item2).Distinct().ToArray());

        [System.ServiceModel.OperationContract]
        [WebGet(UriTemplate = ""?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public QueryResult<{0}.{1}> GetCommonClaim(string filter, string fparam, string genericfilter, int page, int psize, string sort)
        {{
            object filterObject;
            Rhetos.Dom.DefaultConcepts.FilterCriteria[] genericFilter;
            ServiceLoader.GetFilterParameters(filter, fparam, genericfilter, null, out genericFilter, out filterObject);
            return _serviceLoader.GetData<{0}.{1}>(filterObject, genericFilter, page, psize, sort);
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/{{id}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public {0}.{1} Get{0}{1}ById(string id)
        {{
            var filter = new [] {{ Guid.Parse(id) }};

            var result = GetData<{0}.{1}>(filter).Records.FirstOrDefault();
            if (result == null)
                throw new WebFaultException<string>(""There is no resource of this type with a given ID."", HttpStatusCode.NotFound);

            return result;
        }}

        [OperationContract]
        [WebInvoke(Method = ""POST"", UriTemplate = """", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public InsertDataResult Insert{0}{1}({0}.{1} entity)
        {{
            if (Guid.Empty == entity.ID)
                entity.ID = Guid.NewGuid();

            var result = _serviceLoader.InsertData(entity);
            return new InsertDataResult {{ ID = entity.ID }};
        }}

        [OperationContract]
        [WebInvoke(Method = ""PUT"", UriTemplate = ""{{id}}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Update{0}{1}(string id, {0}.{1} entity)
        {{
            Guid guid = Guid.Parse(id);
            if (Guid.Empty == entity.ID)
                entity.ID = guid;
            if (guid != entity.ID)
                throw new WebFaultException<string>(""Given entity ID is not equal to resource ID from URI."", HttpStatusCode.BadRequest);

            _serviceLoader.UpdateData(entity);
        }}

        [OperationContract]
        [WebInvoke(Method = ""DELETE"", UriTemplate = ""{{id}}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Delete{0}{1}(string id)
        {{
            var entity = new {0}.{1} {{ ID = Guid.Parse(id) }};

            _serviceLoader.DeleteData(entity);
        }}

    }}

    ",
            info.Module.Name,
            info.Name);
        }
        
        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            if (IsTypeSupported(info))
            {
                codeBuilder.InsertCode(ServiceRegistrationCodeSnippet(info), InitialCodeGenerator.ServiceRegistrationTag);
            }
        }

    }
}