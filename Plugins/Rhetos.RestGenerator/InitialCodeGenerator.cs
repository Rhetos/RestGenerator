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
using Rhetos.Utilities;
using System;
using System.Web.Routing;
using System.Xml;

namespace Rhetos.RestGenerator
{
    public class InitialCodeGenerator : IRestGeneratorPlugin
    {
        public static readonly string UsingTag = "/*InitialCodeGenerator.UsingTag*/";
        // Keeping 'const' for backward compatibility:
        public const string RhetosRestClassesTag = "/*InitialCodeGenerator.RhetosRestClassesTag*/";
        public const string ServiceRegistrationTag = "/*InitialCodeGenerator.ServiceRegistrationTag*/";
        public const string ServiceInitializationTag = "/*InitialCodeGenerator.ServiceInitializationTag*/";
        public static readonly string ServiceInstanceInitializationTag = "/*InitialCodeGenerator.ServiceInstanceInitializationTag*/";
        public static readonly string ServiceHostOnOpeningBeginTag = "/*InitialCodeGenerator.ServiceHostOnOpeningBeginTag*/";
        public static readonly string ServiceHostOnOpeningTag = "/*InitialCodeGenerator.ServiceHostOnOpeningTag*/";
        public static readonly string ServiceHostOnOpeningEndTag = "/*InitialCodeGenerator.ServiceHostOnOpeningEndTag*/";
        public static readonly string ServiceHostOnOpeningDefaultBindingTag = "/*InitialCodeGenerator.ServiceHostOnOpeningDefaultBindingTag*/";
        public static readonly string DataRestServiceAttributesTag = "/*InitialCodeGenerator.DataRestServiceAttributesTag*/";
        public static readonly string DataRestServicePropertiesTag = "/*InitialCodeGenerator.DataRestServicePropertiesTag*/";
        public static readonly string DataRestServiceConstructorParameterTag = "/*InitialCodeGenerator.DataRestServiceConstructorParameterTag*/";
        public static readonly string DataRestServiceConstructorTag = "/*InitialCodeGenerator.DataRestServiceConstructorTag*/";
        public static readonly string DataRestServiceMethodsTag = "/*InitialCodeGenerator.DataRestServiceMethodsTag*/";
        public static readonly string ActionRestServiceAttributesTag = "/*InitialCodeGenerator.ActionRestServiceAttributesTag*/";
        public static readonly string ActionRestServicePropertiesTag = "/*InitialCodeGenerator.ActionRestServicePropertiesTag*/";
        public static readonly string ActionRestServiceConstructorParameterTag = "/*InitialCodeGenerator.ActionRestServiceConstructorParameterTag*/";
        public static readonly string ActionRestServiceConstructorTag = "/*InitialCodeGenerator.ActionRestServiceConstructorTag*/";
        public static readonly string ActionRestServiceMethodsTag = "/*InitialCodeGenerator.ActionRestServiceMethodsTag*/";
        public static readonly string ReportRestServiceAttributesTag = "/*InitialCodeGenerator.ReportRestServiceAttributesTag*/";
        public static readonly string ReportRestServicePropertiesTag = "/*InitialCodeGenerator.ReportRestServicePropertiesTag*/";
        public static readonly string ReportRestServiceConstructorParameterTag = "/*InitialCodeGenerator.ReportRestServiceConstructorParameterTag*/";
        public static readonly string ReportRestServiceConstructorTag = "/*InitialCodeGenerator.ReportRestServiceConstructorTag*/";
        public static readonly string ReportRestServiceMethodsTag = "/*InitialCodeGenerator.ReportRestServiceMethodsTag*/";
        public static readonly string RestServiceMetadataMembersTag = "/*InitialCodeGenerator.RestServiceMetadataMembersTag*/";
        public static readonly string WritableDataStructuresTag = "/*InitialCodeGenerator.WritableDataStructuresTag*/";
        

        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {

            string CodeSnippet =
$@"#pragma warning disable // Ignore unused namespaces.
using Autofac;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.RestGenerator.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Net;
using System.Text;
{UsingTag}
#pragma warning restore // Ignore unused namespaces.

namespace RestService
{{
    public class RestServiceHostFactory : Autofac.Integration.Wcf.AutofacServiceHostFactory
    {{
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {{
            return new RestServiceHost(serviceType, baseAddresses);
        }}
    }}

    public class RestServiceHost : WebServiceHost
    {{
        public RestServiceHost(Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses) {{ }}

        protected override void OnOpening()
        {{
            {ServiceHostOnOpeningBeginTag}
            var wcfCreatesDefaultBindingsOnOpening = Description.Endpoints.Count == 0;
            // WebServiceHost will automatically create HTTP and HTTPS REST-like endpoints/binding/behaviors pairs, if service endpoint/binding/behavior configuration is empty 
            // After OnOpening setup, we will setup default binding sizes, if needed
            base.OnOpening();
            {ServiceHostOnOpeningTag}

            if (wcfCreatesDefaultBindingsOnOpening)
            {{
                const int sizeInBytes = 200 * 1024 * 1024;
                foreach (var binding in Description.Endpoints.Select(x => x.Binding as WebHttpBinding))
                {{
                    binding.MaxReceivedMessageSize = sizeInBytes;
                    binding.ReaderQuotas.MaxArrayLength = sizeInBytes;
                    binding.ReaderQuotas.MaxStringContentLength = sizeInBytes;
                    {ServiceHostOnOpeningDefaultBindingTag}
                }}
            }}

            if (Description.Behaviors.Find<Rhetos.Web.JsonErrorServiceBehavior>() == null)
                Description.Behaviors.Add(new Rhetos.Web.JsonErrorServiceBehavior());
            {ServiceHostOnOpeningEndTag}
        }}
    }}

    [System.ComponentModel.Composition.Export(typeof(Module))]
    public class RestServiceModuleConfiguration : Module
    {{
        protected override void Load(ContainerBuilder builder)
        {{
            builder.RegisterType<QueryParameters>().InstancePerLifetimeScope();
            builder.RegisterType<ServiceUtility>().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(DataRestService<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(ActionRestService<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(ReportRestService<>)).InstancePerLifetimeScope();
            {ServiceRegistrationTag}
            base.Load(builder);
        }}
    }}

    [System.ComponentModel.Composition.Export(typeof(Rhetos.IService))]
    public class RestServiceInitializer : Rhetos.IService
    {{
        public void Initialize()
        {{
            {ServiceInitializationTag}
        }}

        public void InitializeApplicationInstance(System.Web.HttpApplication context)
        {{
            {ServiceInstanceInitializationTag}
        }}
    }}

    public static class RestServiceMetadata
    {{
        #pragma warning disable CA1825 // Avoid zero-length array allocations. Some metadata can be empty, available to be extended by plugins.

        {RestServiceMetadataMembersTag}

        // This is separated from generic rest service class, because a static field in a generic type is not shared among instances of different close constructed types.
        private static readonly ConcurrentDictionary<string, Tuple<string, Type>[]> FilterTypesByDataStructure = new ConcurrentDictionary<string, Tuple<string, Type>[]>();

        public static Tuple<string,Type>[] GetFilterTypesByDataStructure(string dataStructureName) => FilterTypesByDataStructure.GetOrAdd(dataStructureName, GetFilterTypesByDataStructureName);

        private static Tuple<string, Type>[] GetFilterTypesByDataStructureName(string dataStructureName)
        {{
            return typeof(RestServiceMetadata)
                .GetMethod($""Get_{{ dataStructureName.Replace('.', '_')}}_FilterTypes"")
                  .Invoke(null, Array.Empty<object>()) as Tuple<string, Type>[];
        }}

        public static readonly HashSet<string> WritableDataStructures = new HashSet<string>(new string[]
        {{
            {WritableDataStructuresTag}
        }});

        #pragma warning restore CA1825 // Avoid zero-length array allocations.
    }}

#pragma warning disable CS0618 // 'LegacyClientException' is obsolete: 'Use ClientException instead.'

    {DataRestServiceAttributesTag}
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DataRestService<TDataStructure> where TDataStructure : class, IEntity, new()
    {{
        private readonly ServiceUtility _serviceUtility;
        {DataRestServicePropertiesTag}

        public DataRestService(ServiceUtility serviceUtility{DataRestServiceConstructorParameterTag})
        {{
            _serviceUtility = serviceUtility;
            {DataRestServiceConstructorTag}
        }}

        // Obsolete parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&top={{top}}&skip={{skip}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public RecordsResult<TDataStructure> Get(string filter, string fparam, string genericfilter, string filters, int top, int skip, int page, int psize, string sort)
        {{
            var data = _serviceUtility.GetData<TDataStructure>(filter, fparam, genericfilter, filters, RestServiceMetadata.GetFilterTypesByDataStructure(typeof(TDataStructure).FullName), top, skip, page, psize, sort,
                readRecords: true, readTotalCount: false);
            return new RecordsResult<TDataStructure> {{ Records = data.Records }};
        }}

        [Obsolete(""Use GetTotalCount instead."")]
        [OperationContract]
        [WebGet(UriTemplate = ""/Count?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public CountResult GetCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {{
            var data = _serviceUtility.GetData<TDataStructure>(filter, fparam, genericfilter, filters, RestServiceMetadata.GetFilterTypesByDataStructure(typeof(TDataStructure).FullName), 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new CountResult {{ TotalRecords = data.TotalCount }};
        }}

        // Obsolete parameters: filter, fparam, genericfilter (use filters).
        [OperationContract]
        [WebGet(UriTemplate = ""/TotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public TotalCountResult GetTotalCount(string filter, string fparam, string genericfilter, string filters, string sort)
        {{
            var data = _serviceUtility.GetData<TDataStructure>(filter, fparam, genericfilter, filters, RestServiceMetadata.GetFilterTypesByDataStructure(typeof(TDataStructure).FullName), 0, 0, 0, 0, sort,
                readRecords: false, readTotalCount: true);
            return new TotalCountResult {{ TotalCount = data.TotalCount }};
        }}

        // Obsolete parameters: filter, fparam, genericfilter (use filters), page, psize (use top and skip).
        [OperationContract]
        [WebGet(UriTemplate = ""/RecordsAndTotalCount?filter={{filter}}&fparam={{fparam}}&genericfilter={{genericfilter}}&filters={{filters}}&top={{top}}&skip={{skip}}&page={{page}}&psize={{psize}}&sort={{sort}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public RecordsAndTotalCountResult<TDataStructure> GetRecordsAndTotalCount(string filter, string fparam, string genericfilter, string filters, int top, int skip, int page, int psize, string sort)
        {{
            return _serviceUtility.GetData<TDataStructure>(filter, fparam, genericfilter, filters, RestServiceMetadata.GetFilterTypesByDataStructure(typeof(TDataStructure).FullName), top, skip, page, psize, sort,
                readRecords: true, readTotalCount: true);
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/{{id}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public TDataStructure GetById(string id)
        {{
            var result = _serviceUtility.GetDataById<TDataStructure>(id);
            if (result == null)
                throw new Rhetos.LegacyClientException(""There is no resource of this type with a given ID."") {{ HttpStatusCode = HttpStatusCode.NotFound, Severe = false }};
            return result;
        }}

        [OperationContract]
        [WebInvoke(Method = ""POST"", UriTemplate = """", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public InsertDataResult Insert(TDataStructure entity)
        {{
            if (!RestServiceMetadata.WritableDataStructures.Contains(typeof(TDataStructure).FullName))
                throw new Rhetos.ClientException($""Invalid request: '{{typeof(TDataStructure).FullName}}' is not writable."");
            if (entity == null)
                throw new Rhetos.ClientException(""Invalid request: Missing the record data. The data should be provided in the request message body."");
            if (Guid.Empty == entity.ID)
                entity.ID = Guid.NewGuid();

            _serviceUtility.InsertData(entity);
            return new InsertDataResult {{ ID = entity.ID }};
        }}

        [OperationContract]
        [WebInvoke(Method = ""PUT"", UriTemplate = ""{{id}}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Update(string id, TDataStructure entity)
        {{
            if (!RestServiceMetadata.WritableDataStructures.Contains(typeof(TDataStructure).FullName))
                throw new Rhetos.ClientException($""Invalid request: '{{typeof(TDataStructure).FullName}}' is not writable."");
            if (entity == null)
                throw new Rhetos.ClientException(""Invalid request: Missing the record data. The data should be provided in the request message body."");
            if (!Guid.TryParse(id, out Guid guid))
                throw new Rhetos.LegacyClientException(""Invalid format of GUID parameter 'ID'."");
            if (Guid.Empty == entity.ID)
                entity.ID = guid;
            if (guid != entity.ID)
                throw new Rhetos.LegacyClientException(""Given entity ID is not equal to resource ID from URI."");

            _serviceUtility.UpdateData(entity);
        }}

        [OperationContract]
        [WebInvoke(Method = ""DELETE"", UriTemplate = ""{{id}}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Delete(string id)
        {{
            if (!RestServiceMetadata.WritableDataStructures.Contains(typeof(TDataStructure).FullName))
                throw new Rhetos.ClientException($""Invalid request: '{{typeof(TDataStructure).FullName}}' is not writable."");
            if (!Guid.TryParse(id, out Guid guid))
                throw new Rhetos.LegacyClientException(""Invalid format of GUID parameter 'ID'."");
            var entity = new TDataStructure {{ ID = guid }};

            _serviceUtility.DeleteData(entity);
        }}

        {DataRestServiceMethodsTag}
    }}

    {ActionRestServiceAttributesTag}
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ActionRestService<TDataStructure> where TDataStructure : class, new()
    {{
        private readonly ServiceUtility _serviceUtility;
        {ActionRestServicePropertiesTag}

        public ActionRestService(ServiceUtility serviceUtility{ActionRestServiceConstructorParameterTag})
        {{
            _serviceUtility = serviceUtility;
            {ActionRestServiceConstructorTag}
        }}

        [OperationContract]
        [WebInvoke(Method = ""POST"", UriTemplate = """", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Execute(TDataStructure action)
        {{
            _serviceUtility.Execute<TDataStructure>(action);
        }}

        {ActionRestServiceMethodsTag}
    }}

    {ReportRestServiceAttributesTag}
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ReportRestService<TDataStructure> where TDataStructure : class, new()
    {{
        private readonly ServiceUtility _serviceUtility;
        {ReportRestServicePropertiesTag}

        public ReportRestService(ServiceUtility serviceUtility{ReportRestServiceConstructorParameterTag})
        {{
            _serviceUtility = serviceUtility;
            {ReportRestServiceConstructorTag}
        }}

        [OperationContract]
        [WebGet(UriTemplate = ""/?parameter={{parameter}}&convertFormat={{convertFormat}}"", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public DownloadReportResult DownloadReport(string parameter, string convertFormat)
        {{
            return _serviceUtility.DownloadReport<TDataStructure>(parameter, convertFormat);
        }}

        {ReportRestServiceMethodsTag}
    }}

    {RhetosRestClassesTag}

#pragma warning restore CS0618 // 'LegacyClientException' is obsolete: 'Use ClientException instead.'
}}
";

            codeBuilder.InsertCode(CodeSnippet);

            // Global
            codeBuilder.AddReferencesFromDependency(typeof(Guid));
            codeBuilder.AddReferencesFromDependency(typeof(System.Linq.Enumerable));
            codeBuilder.AddReferencesFromDependency(typeof(System.Configuration.ConfigurationElement));
            codeBuilder.AddReferencesFromDependency(typeof(System.Diagnostics.Stopwatch));
            codeBuilder.AddReferencesFromDependency(typeof(XmlReader));
            
            // Registration
            codeBuilder.AddReferencesFromDependency(typeof(System.ComponentModel.Composition.ExportAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(Autofac.Integration.Wcf.AutofacServiceHostFactory));

            // WCF Data Services
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.ServiceContractAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Activation.AspNetCompatibilityRequirementsAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Web.WebServiceHost));
            codeBuilder.AddReferencesFromDependency(typeof(System.Uri));
            codeBuilder.AddReferencesFromDependency(typeof(System.Web.Routing.RouteTable));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Activation.ServiceHostFactory));
            codeBuilder.AddReferencesFromDependency(typeof(Route));
            codeBuilder.AddReferencesFromDependency(typeof(XmlDictionaryReaderQuotas)); // System.Runtime.Serialization, needed for "binding.ReaderQuotas" in code snippet above.

            // Rhetos
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.IService));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Dom.DefaultConcepts.IEntity));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Logging.ILogger));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Logging.LoggerHelper));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Processing.IProcessingEngine));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Utilities.XmlUtility));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.XmlSerialization.XmlData));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Web.JsonErrorServiceBehavior));

            // RestGenerator
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.RestGenerator.Utilities.ServiceUtility));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.RestGenerator.Utilities.DownloadReportResult));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Processing.DefaultCommands.ReadCommandResult));
            codeBuilder.AddReferencesFromDependency(typeof(Newtonsoft.Json.JsonConvert));
            codeBuilder.AddReferencesFromDependency(typeof(Newtonsoft.Json.Linq.JToken));

            foreach (var file in Paths.DomAssemblyFiles)
                codeBuilder.AddReference(file);
            codeBuilder.AddReference(typeof(Autofac.Module).Assembly.Location);
        }
    }
}
