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
using Rhetos.Web;
using Rhetos.Compiler;
using Rhetos.Dsl;
using System.IO;
using System.ServiceModel;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Net;
using System.Runtime.Serialization;
using System.Web.Routing;
using System.ServiceModel.Description;

namespace Rhetos.RestGenerator
{
    public class InitialCodeGenerator : IRestGeneratorPlugin
    {
        public const string RhetosRestClassesTag = "/*InitialCodeGenerator.RhetosRestClassesTag*/";
        public const string ServiceRegistrationTag = "/*InitialCodeGenerator.ServiceRegistrationTag*/";
        public const string ServiceInitializationTag = "/*InitialCodeGenerator.ServiceInitializationTag*/";
        public const string ServiceLoaderMembersTag = "/*InitialCodeGenerator.ServiceLoaderMembersTag*/";
        

        private const string CodeSnippet =
@"
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
using Autofac;
using Rhetos.Logging;
using Rhetos.Processing;
using Rhetos.XmlSerialization;
using Rhetos.Dom.DefaultConcepts;
using System.Runtime.Serialization.Json;
using Rhetos.Processing.DefaultCommands;
using System.Web.Routing;
using Module = Autofac.Module;

namespace Rhetos.Rest
{
    public class RestServiceHostFactory : Autofac.Integration.Wcf.AutofacServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            RestServiceHost host = new RestServiceHost(serviceType, baseAddresses);

            return host;
        }
    }

    public class RestServiceHost : ServiceHost
    {
        private Type _serviceType;

        public RestServiceHost(Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            _serviceType = serviceType;
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.AddServiceEndpoint(_serviceType, new WebHttpBinding(""rhetosWebHttpBinding""), string.Empty);
            this.AddServiceEndpoint(_serviceType, new BasicHttpBinding(""rhetosBasicHttpBinding""), ""SOAP"");

            ((ServiceEndpoint)(Description.Endpoints.Where(e => e.Binding is WebHttpBinding).Single())).Behaviors.Add(new WebHttpBehavior()); 
            if (Description.Behaviors.Find<Rhetos.Web.JsonErrorServiceBehavior>() == null)
                Description.Behaviors.Add(new Rhetos.Web.JsonErrorServiceBehavior());
        }
    }

    [System.ComponentModel.Composition.Export(typeof(Module))]
    public class RestServiceModuleConfiguration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServiceLoader>().InstancePerLifetimeScope();
            " + ServiceRegistrationTag + @"
            base.Load(builder);
        }
    }

    [System.ComponentModel.Composition.Export(typeof(Rhetos.IService))]
    public class RestServiceInitializer : Rhetos.IService
    {
        public void Initialize()
        {
            " + ServiceInitializationTag + @"
        }

        public void InitializeApplicationInstance(System.Web.HttpApplication context)
        {
        }
    }

    public class MessagesResult 
    {
        public string SystemMessage;
        public string UserMessage;

        public override string ToString() 
        {
            return ""SystemMessage: "" + (SystemMessage ?? ""<null>"") + "", UserMessage: "" + (UserMessage ?? ""<null>"");
        }
    } 

    public class ServiceLoader
    {
        private Rhetos.Processing.IProcessingEngine _processingEngine;

        private Rhetos.Logging.ILogger _logger;
        private Rhetos.Logging.ILogger _commandsLogger;
        private Rhetos.Logging.ILogger _performanceLogger;
        private System.Diagnostics.Stopwatch _stopwatch;

        private static void CheckForErrors(ProcessingResult result)
        {
            if (!result.Success)
                throw new WebFaultException<MessagesResult>(
                        new MessagesResult { SystemMessage = result.SystemMessage, UserMessage = result.UserMessage }, 
                        string.IsNullOrEmpty(result.UserMessage) ? HttpStatusCode.InternalServerError : HttpStatusCode.BadRequest
                );
        }

        public ServiceLoader(
            Rhetos.Processing.IProcessingEngine processingEngine,
            Rhetos.Logging.ILogProvider logProvider) 
        {
            _processingEngine = processingEngine;
            _logger = logProvider.GetLogger(""RestService"");
            _commandsLogger = logProvider.GetLogger(""RestService Commands"");
            _performanceLogger = logProvider.GetLogger(""Performance"");
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.Trace(""Rest Service loader initialized."");
        }

" + ServiceLoaderMembersTag + @"
    }

" + RhetosRestClassesTag + @"

}
";
        
        private static readonly string _rootPath = AppDomain.CurrentDomain.BaseDirectory;

        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            codeBuilder.InsertCode(CodeSnippet);

            // global
            codeBuilder.AddReferencesFromDependency(typeof(Guid));
            codeBuilder.AddReferencesFromDependency(typeof(System.Linq.Enumerable));
            codeBuilder.AddReferencesFromDependency(typeof(System.Configuration.ConfigurationElement));
            codeBuilder.AddReferencesFromDependency(typeof(System.Diagnostics.Stopwatch));
            codeBuilder.AddReferencesFromDependency(typeof(XmlReader));
            
            // registration
            codeBuilder.AddReferencesFromDependency(typeof(System.ComponentModel.Composition.ExportAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(Autofac.Integration.Wcf.AutofacServiceHostFactory));

            // wcf dataservices
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.ServiceContractAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Activation.AspNetCompatibilityRequirementsAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Web.WebServiceHost));
            codeBuilder.AddReferencesFromDependency(typeof(System.Uri));
            codeBuilder.AddReferencesFromDependency(typeof(System.Web.Routing.RouteTable));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Activation.ServiceHostFactory));
            codeBuilder.AddReferencesFromDependency(typeof(Route));
            
            // rhetos
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Processing.DefaultCommands.QueryDataSourceCommandResult));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.IService));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Dom.DefaultConcepts.IEntity));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Logging.ILogger));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Logging.LoggerHelper));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Processing.IProcessingEngine));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.UserException));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Utilities.XmlUtility));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.XmlSerialization.XmlData));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Web.JsonErrorServiceBehavior));

            codeBuilder.AddReference(Path.Combine(_rootPath, "ServerDom.dll"));
            codeBuilder.AddReference(Path.Combine(_rootPath, "Autofac.dll"));
        }

    }

}
