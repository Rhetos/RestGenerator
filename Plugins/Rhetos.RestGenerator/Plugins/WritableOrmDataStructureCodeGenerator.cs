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

namespace Rhetos.RestGenerator.Plugins
{
    /// <summary>
    /// This is not exported, but called from DataStructureCodeGenerator if exists.
    /// </summary>
    public static class WritableOrmDataStructureCodeGenerator
    {      
        public static void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            DataStructureInfo info = (DataStructureInfo) conceptInfo;

            if (info is IWritableOrmDataStructure)
            {
                string snippet = $@"
        [OperationContract]
        [WebInvoke(Method = ""POST"", UriTemplate = """", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public InsertDataResult Insert{info.Module.Name}{info.Name}({info.Module.Name}.{info.Name} entity)
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
        public void Update{info.Module.Name}{info.Name}(string id, {info.Module.Name}.{info.Name} entity)
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
        public void Delete{info.Module.Name}{info.Name}(string id)
        {{
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                throw new Rhetos.LegacyClientException(""Invalid format of GUID parameter 'ID'."");
            var entity = new {info.Module.Name}.{info.Name} {{ ID = guid }};

            _serviceUtility.DeleteData(entity);
        }}

";
                codeBuilder.InsertCode(snippet, DataStructureCodeGenerator.AdditionalOperationsTag.Evaluate(info));
            }
        }
    }
}