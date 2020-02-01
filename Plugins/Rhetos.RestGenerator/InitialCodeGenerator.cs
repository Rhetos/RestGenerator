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
        public const string RhetosRestClassesTag = "/*InitialCodeGenerator.RhetosRestClassesTag*/";
        public const string ServiceRegistrationTag = "/*InitialCodeGenerator.ServiceRegistrationTag*/";
        public const string ServiceInitializationTag = "/*InitialCodeGenerator.ServiceInitializationTag*/";
        public const string ServiceHostOnOpeningBeginTag = "/*ServiceHostOnOpeningBegin*/";
        public const string ServiceHostOnOpeningEndTag = "/*ServiceHostOnOpeningEnd*/";

        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            string CodeSnippet =
$@"
using Autofac;
using Module = Autofac.Module;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.RestGenerator.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Routing;

namespace Rhetos.Rest
{{
    public class RestServiceHostFactory : Autofac.Integration.Wcf.AutofacServiceHostFactory
    {{
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {{
            RestServiceHost host = new RestServiceHost(serviceType, baseAddresses);

            return host;
        }}
    }}

    public class RestServiceHost : WebServiceHost
    {{
        public RestServiceHost(Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses) {{ }}

        protected override void OnOpening()
        {{
            {ServiceHostOnOpeningBeginTag}
            var setupDefaultBindingSizes = Description.Endpoints.Count == 0;
            // WebServiceHost will automatically create HTTP and HTTPS REST-like endpoints/binding/behaviours pairs, if service endpoint/binding/behaviour configuration is empty 
            // After OnOpening setup, we will setup default binding sizes, if needed
            base.OnOpening();

            if (setupDefaultBindingSizes)
            {{
                const int sizeInBytes = 209715200;
                foreach (var binding in Description.Endpoints.Select(x => x.Binding as WebHttpBinding))
                {{
                    binding.MaxReceivedMessageSize = sizeInBytes;
                    binding.ReaderQuotas.MaxArrayLength = sizeInBytes;
                    binding.ReaderQuotas.MaxStringContentLength = sizeInBytes;
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
        }}
    }}

{RhetosRestClassesTag}

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

            foreach (var file in Paths.DomAssemblyFiles)
                codeBuilder.AddReference(file);
            codeBuilder.AddReference(typeof(Autofac.Module).Assembly.Location);
        }
    }
}
