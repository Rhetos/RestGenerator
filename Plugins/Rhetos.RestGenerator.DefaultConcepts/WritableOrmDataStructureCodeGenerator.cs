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
using Rhetos.Compiler;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;
using Rhetos.RestGenerator;

namespace Rhetos.RestGenerator.DefaultConcepts
{
    /// <summary>
    /// This is not exported, but called from DataStructureCodeGenerator if exists.
    /// </summary>
    public class WritableOrmDataStructureCodeGenerator
    {      
        private const string ImplementationCodeSnippet = @"
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

";

        public static void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            GenerateInitialCode(codeBuilder);

            DataStructureInfo info = (DataStructureInfo) conceptInfo;

            if (info is IWritableOrmDataStructure)
            {
                codeBuilder.InsertCode(
                    String.Format(ImplementationCodeSnippet, info.Module.Name, info.Name),
                    DataStructureCodeGenerator.AdditionalOperationsTag.Evaluate(info));
            }
        }

        private static bool _isInitialCallMade;

        public static void GenerateInitialCode(ICodeBuilder codeBuilder)
        {
            if (_isInitialCallMade)
                return;
            _isInitialCallMade = true;

            codeBuilder.InsertCode(@"

        public class InsertDataResult
        {
            public Guid ID;
        }", InitialCodeGenerator.RhetosRestClassesTag);

            codeBuilder.InsertCode(@"

        public ProcessingResult InsertData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
                                  {
                                      Entity = typeof(T).FullName,
                                      DataToInsert = new[] { (IEntity)entity }
                                  };

            var result = _processingEngine.Execute(new[]{commandInfo});
            CheckForErrors(result);

            return result;
        }

        public ProcessingResult UpdateData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToUpdate = new[] { (IEntity)entity }
            };

            var result = _processingEngine.Execute(new[]{commandInfo});
            CheckForErrors(result);

            return result;
        }

        public ProcessingResult DeleteData<T>(T entity)
        {
            var commandInfo = new SaveEntityCommandInfo
            {
                Entity = typeof(T).FullName,
                DataToDelete = new[] { (IEntity)entity }
            };

            var result = _processingEngine.Execute(new[]{commandInfo});
            CheckForErrors(result);

            return result;
        }

", InitialCodeGenerator.ServiceLoaderMembersTag);

        }
    }
}