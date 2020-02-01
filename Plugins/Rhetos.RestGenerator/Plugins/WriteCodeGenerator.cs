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
    [ExportMetadata(MefProvider.Implements, typeof(WriteInfo))]
    public class WriteCodeGenerator : IRestGeneratorPlugin
    {
        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            WriteInfo info = (WriteInfo)conceptInfo;

            string snippet = $@"
        [OperationContract]
        [WebInvoke(Method = ""POST"", UriTemplate = """", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public InsertDataResult Insert{info.DataStructure.Module.Name}{info.DataStructure.Name}({info.DataStructure.Module.Name}.{info.DataStructure.Name} entity)
        {{
            if (entity == null)
                throw new Rhetos.ClientException(""Invalid request: Missing the record data. The data should be provided in the request message body."");
            if (Guid.Empty == entity.ID)
                entity.ID = Guid.NewGuid();

            var result = _serviceUtility.InsertData(entity);
            return new InsertDataResult {{ ID = entity.ID }};
        }}

        [OperationContract]
        [WebInvoke(Method = ""PUT"", UriTemplate = ""{{id}}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Update{info.DataStructure.Module.Name}{info.DataStructure.Name}(string id, {info.DataStructure.Module.Name}.{info.DataStructure.Name} entity)
        {{
            if (entity == null)
                throw new Rhetos.ClientException(""Invalid request: Missing the record data. The data should be provided in the request message body."");
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                throw new Rhetos.LegacyClientException(""Invalid format of GUID parameter 'ID'."");
            if (Guid.Empty == entity.ID)
                entity.ID = guid;
            if (guid != entity.ID)
                throw new Rhetos.LegacyClientException(""Given entity ID is not equal to resource ID from URI."");

            _serviceUtility.UpdateData(entity);
        }}

        [OperationContract]
        [WebInvoke(Method = ""DELETE"", UriTemplate = ""{{id}}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Delete{info.DataStructure.Module.Name}{info.DataStructure.Name}(string id)
        {{
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                throw new Rhetos.LegacyClientException(""Invalid format of GUID parameter 'ID'."");
            var entity = new {info.DataStructure.Module.Name}.{info.DataStructure.Name} {{ ID = guid }};

            _serviceUtility.DeleteData(entity);
        }}

";
            codeBuilder.InsertCode(
                    snippet,
                DataStructureCodeGenerator.AdditionalOperationsTag.Evaluate(info.DataStructure));
        }

    }
}